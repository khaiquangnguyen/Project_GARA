using System;
using System.Collections.Generic;
using GARA.Input;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>
    /// Drives the on-screen presentation of a <see cref="RhythmSequenceRunner"/>: spawns a
    /// traveling arrow per note ahead of its hit time, shrinks each note's approach circle
    /// to land on that note's hit time, and plays an optional <see cref="MMF_Player"/> at each key
    /// moment (sequence start/end, note spawn, window open, each judgement). Purely visual — all timing/judging stays in
    /// <see cref="RhythmSequenceRunner"/>; this only reads it. Lives in the scene, not on a
    /// character: whoever owns the scene's visuals calls <see cref="Show"/> and
    /// <see cref="Hide"/> as sequences start and end.
    /// </summary>
    public class RhythmVisualDriver : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private RhythmVisualSpec spec;

        [Tooltip("Pool of RhythmNoteView objects (its GameObjectToPool is the note prefab). Notes return to it by disabling themselves at the end of their hit/miss feedback.")]
        [SerializeField]
        private MMSimpleObjectPooler notePool;

        [SerializeField]
        private Transform spawnPoint;

        [SerializeField]
        private Transform targetPoint;

        [Tooltip("Optional. Shown only while a sequence is being presented — enabled by Show, disabled by Hide.")]
        [SerializeField]
        private GameObject background;

        [Header("Feedbacks (optional)")]
        [Tooltip("Played at the target point when a sequence starts showing.")]
        [SerializeField]
        private MMF_Player sequenceStartedFeedback;

        [Tooltip("Played at the target point when a sequence finishes or is aborted.")]
        [SerializeField]
        private MMF_Player sequenceEndedFeedback;

        [Tooltip("Played at the spawn point each time a note appears.")]
        [SerializeField]
        private MMF_Player noteSpawnedFeedback;

        [Tooltip("Played at the target point when a note's timing window opens.")]
        [SerializeField]
        private MMF_Player windowOpenedFeedback;

        [Tooltip("Played where the note was on any hit (Perfect, Good or Ok), in addition to that judgement's own feedback.")]
        [SerializeField]
        private MMF_Player hitFeedback;

        [Tooltip("Played where the note was when it was judged Perfect.")]
        [SerializeField]
        private MMF_Player perfectFeedback;

        [Tooltip("Played where the note was when it was judged Good.")]
        [SerializeField]
        private MMF_Player goodFeedback;

        [Tooltip("Played where the note was when it was judged Ok.")]
        [SerializeField]
        private MMF_Player okFeedback;

        [Tooltip("Played where the note was when it was judged a Miss.")]
        [SerializeField]
        private MMF_Player missFeedback;

        private readonly Dictionary<int, RhythmNoteView> _activeNotes = new Dictionary<int, RhythmNoteView>();
        private readonly HashSet<int> _warnedUnmappedTokenIds = new HashSet<int>();
        private RhythmSequenceRunner _boundRunner;
        private bool[] _spawned;

        public float TravelDuration => spec.TravelDuration;

        private void Awake()
        {
            if (spec == null)
            {
                throw new InvalidOperationException($"{nameof(RhythmVisualDriver)} on '{name}' has no visual spec assigned.");
            }

            if (notePool == null)
            {
                throw new InvalidOperationException($"{nameof(RhythmVisualDriver)} on '{name}' has no note pool assigned.");
            }

            if (spawnPoint == null)
            {
                throw new InvalidOperationException($"{nameof(RhythmVisualDriver)} on '{name}' has no spawn point assigned.");
            }

            if (targetPoint == null)
            {
                throw new InvalidOperationException($"{nameof(RhythmVisualDriver)} on '{name}' has no target point assigned.");
            }

            SetBackgroundVisible(false);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>Starts presenting <paramref name="runner"/>, replacing whatever was shown before.</summary>
        public void Show(RhythmSequenceRunner runner)
        {
            Unbind();

            if (runner == null)
            {
                return;
            }

            _boundRunner = runner;
            _spawned = new bool[_boundRunner.Definition.Notes.Count];
            _boundRunner.NoteWindowOpened += HandleWindowOpened;
            _boundRunner.NoteJudged += HandleNoteJudged;
            SetBackgroundVisible(true);
            PlayFeedback(sequenceStartedFeedback, targetPoint.position);
        }

        /// <summary>Clears every note still traveling.</summary>
        public void Hide()
        {
            if (_boundRunner != null)
            {
                PlayFeedback(sequenceEndedFeedback, targetPoint.position);
            }

            Unbind();
        }

        private void Update()
        {
            if (_boundRunner == null || !_boundRunner.IsRunning)
            {
                return;
            }

            SpawnDueNotes(_boundRunner);
            UpdateActiveNotePositions(_boundRunner);
        }

        private void Unbind()
        {
            if (_boundRunner != null)
            {
                _boundRunner.NoteWindowOpened -= HandleWindowOpened;
                _boundRunner.NoteJudged -= HandleNoteJudged;
                _boundRunner = null;
            }

            foreach (var view in _activeNotes.Values)
            {
                // Null during scene teardown, when the pooled notes may already be gone.
                if (view != null)
                {
                    view.gameObject.SetActive(false);
                }
            }

            _activeNotes.Clear();
            SetBackgroundVisible(false);
        }

        private void SpawnDueNotes(RhythmSequenceRunner runner)
        {
            var notes = runner.Definition.Notes;
            for (var i = 0; i < notes.Count; i++)
            {
                if (_spawned[i])
                {
                    continue;
                }

                var hitTime = runner.Definition.LeadIn + notes[i].time;
                if (runner.ElapsedTime < hitTime - spec.TravelDuration)
                {
                    continue;
                }

                _spawned[i] = true;
                SpawnNote(i);
            }
        }

        private void SpawnNote(int noteIndex)
        {
            var pooled = notePool.GetPooledGameObject();
            if (pooled == null)
            {
                Debug.LogWarning($"{nameof(RhythmVisualDriver)} on '{name}': note pool is exhausted — note {noteIndex} not shown. Raise the pool size or let it expand.", this);
                return;
            }

            pooled.SetActive(true);
            var view = pooled.GetComponent<RhythmNoteView>();
            view.Place(spawnPoint.position, targetPoint.position, NoteRotationFor(_boundRunner.Definition.Notes[noteIndex].input));
            view.SetProgress(0f, spec.ApproachCircleScaleAt(0f));
            _activeNotes[noteIndex] = view;
            PlayFeedback(noteSpawnedFeedback, spawnPoint.position);
        }

        private void UpdateActiveNotePositions(RhythmSequenceRunner runner)
        {
            var notes = runner.Definition.Notes;
            var travelDuration = spec.TravelDuration;
            foreach (var pair in _activeNotes)
            {
                var hitTime = runner.Definition.LeadIn + notes[pair.Key].time;
                var t = travelDuration > 0f ? 1f - (hitTime - runner.ElapsedTime) / travelDuration : 1f;
                pair.Value.SetProgress(t, spec.ApproachCircleScaleAt(t));
            }
        }

        private void HandleWindowOpened(int noteIndex)
        {
            PlayFeedback(windowOpenedFeedback, targetPoint.position);
        }

        private void HandleNoteJudged(RhythmNoteResult result)
        {
            var feedbackPosition = targetPoint.position;
            if (_activeNotes.TryGetValue(result.NoteIndex, out var noteView))
            {
                feedbackPosition = noteView.transform.position;
                _activeNotes.Remove(result.NoteIndex);
                if (result.IsHit)
                {
                    noteView.PlayHit(NoteVelocity());
                }
                else
                {
                    noteView.PlayMiss(NoteVelocity());
                }
            }

            if (result.IsHit)
            {
                PlayFeedback(hitFeedback, feedbackPosition);
            }

            PlayFeedback(FeedbackFor(result.Judgement), feedbackPosition);
        }

        private Quaternion NoteRotationFor(InputToken token)
        {
            if (!spec.TryGetNoteRotation(token, out var rotation) && _warnedUnmappedTokenIds.Add(token.Id))
            {
                Debug.LogWarning($"{nameof(RhythmVisualDriver)} on '{name}': {token} has no direction in the visual spec — its notes keep the sprite's default facing.", this);
            }

            return rotation;
        }

        private Vector3 NoteVelocity()
        {
            var travelDuration = spec.TravelDuration;
            return travelDuration > 0f ? (targetPoint.position - spawnPoint.position) / travelDuration : Vector3.zero;
        }

        private MMF_Player FeedbackFor(RhythmJudgement judgement)
        {
            switch (judgement)
            {
                case RhythmJudgement.Perfect:
                    return perfectFeedback;
                case RhythmJudgement.Good:
                    return goodFeedback;
                case RhythmJudgement.Ok:
                    return okFeedback;
                default:
                    return missFeedback;
            }
        }

        private void SetBackgroundVisible(bool visible)
        {
            if (background != null)
            {
                background.SetActive(visible);
            }
        }

        private static void PlayFeedback(MMF_Player feedback, Vector3 position)
        {
            if (feedback != null)
            {
                feedback.PlayFeedbacks(position);
            }
        }
    }
}
