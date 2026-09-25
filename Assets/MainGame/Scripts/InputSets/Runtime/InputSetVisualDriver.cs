using System;
using System.Collections.Generic;
using GARA.Input;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// Drives the on-screen presentation of an <see cref="InputSetCollectionRunner"/>: only the
    /// current set is ever shown, as a row of tokens spread evenly around the row anchor. It marks
    /// the next token to press, tints accepted ones, resets the row on a retry, and hands the row
    /// off to its exit feedbacks once the set finishes. Also shows the set and set collection timers, and
    /// plays an optional <see cref="MMF_Player"/> at each key moment. Purely visual — all judging
    /// stays in <see cref="InputSetCollectionRunner"/>; this only reads it. Lives in the scene, not
    /// on a character: whoever owns the scene's visuals calls <see cref="Show"/> and
    /// <see cref="Hide"/> as set collections start and end.
    /// </summary>
    public class InputSetVisualDriver : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputSetVisualSpec spec;

        [Tooltip("Pool of InputSetTokenView objects (its GameObjectToPool is the token prefab). Tokens return to it by disabling themselves at the end of their exit feedback.")]
        [SerializeField]
        private MMSimpleObjectPooler tokenPool;

        [Tooltip("Center of the current set's row; tokens are spread evenly along its right axis.")]
        [SerializeField]
        private Transform rowAnchor;

        [Tooltip("Optional. Time left in the current set; hidden when the set has no time limit.")]
        [SerializeField]
        private InputSetTimerView setTimer;

        [Tooltip("Optional. Time left in the whole set collection; hidden when the set collection has no time limit.")]
        [SerializeField]
        private InputSetTimerView setCollectionTimer;

        [Tooltip("Optional. Shown only while a set collection is being presented — enabled by Show, disabled by Hide.")]
        [SerializeField]
        private GameObject background;

        [Header("Feedbacks (optional)")]
        [Tooltip("Played at the row anchor when a set collection starts showing.")]
        [SerializeField]
        private MMF_Player setCollectionStartedFeedback;

        [Tooltip("Played at the row anchor when a set collection finishes or is aborted.")]
        [SerializeField]
        private MMF_Player setCollectionEndedFeedback;

        [Tooltip("Played at the row anchor when a set collection ends because its time limit ran out, in addition to Set Collection Ended.")]
        [SerializeField]
        private MMF_Player setCollectionTimedOutFeedback;

        [Tooltip("Played at the row anchor each time a new set appears (not on retries).")]
        [SerializeField]
        private MMF_Player setStartedFeedback;

        [Tooltip("Played at the row anchor each time a set restarts for a retry.")]
        [SerializeField]
        private MMF_Player setRetriedFeedback;

        [Tooltip("Played at a token when it's pressed correctly.")]
        [SerializeField]
        private MMF_Player inputAcceptedFeedback;

        [Tooltip("Played at the expected token when a wrong token is pressed.")]
        [SerializeField]
        private MMF_Player wrongInputFeedback;

        [Tooltip("Played at the row anchor when the current set's time limit runs out.")]
        [SerializeField]
        private MMF_Player setTimedOutFeedback;

        [Tooltip("Played at the row anchor on any cleared set, in addition to Set Cleared First Try when that applies.")]
        [SerializeField]
        private MMF_Player setClearedFeedback;

        [Tooltip("Played at the row anchor when a set is cleared on its first attempt.")]
        [SerializeField]
        private MMF_Player setClearedFirstTryFeedback;

        [Tooltip("Played at the row anchor when a set runs out of attempts.")]
        [SerializeField]
        private MMF_Player setExhaustedFeedback;

        private readonly List<InputSetTokenView> _row = new List<InputSetTokenView>();
        private readonly HashSet<int> _warnedUnmappedTokenIds = new HashSet<int>();
        private InputSetCollectionRunner _boundRunner;
        private int _rowSetIndex = -1;
        private int _acceptedCount;

        private void Awake()
        {
            if (spec == null)
            {
                throw new InvalidOperationException($"{nameof(InputSetVisualDriver)} on '{name}' has no visual spec assigned.");
            }

            if (tokenPool == null)
            {
                throw new InvalidOperationException($"{nameof(InputSetVisualDriver)} on '{name}' has no token pool assigned.");
            }

            if (rowAnchor == null)
            {
                throw new InvalidOperationException($"{nameof(InputSetVisualDriver)} on '{name}' has no row anchor assigned.");
            }

            SetTimersVisible(false);
            SetBackgroundVisible(false);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// Starts presenting <paramref name="runner"/>, replacing whatever was shown before. Call it
        /// before or after the runner's Start() — an already-running runner's current set is shown
        /// straight away.
        /// </summary>
        public void Show(InputSetCollectionRunner runner)
        {
            Unbind();

            if (runner == null)
            {
                return;
            }

            _boundRunner = runner;
            _boundRunner.SetStarted += HandleSetStarted;
            _boundRunner.InputAccepted += HandleInputAccepted;
            _boundRunner.SetFailed += HandleSetFailed;
            _boundRunner.SetFinished += HandleSetFinished;
            SetBackgroundVisible(true);
            PlayFeedback(setCollectionStartedFeedback, rowAnchor.position);

            if (_boundRunner.IsRunning)
            {
                BuildRow(_boundRunner.CurrentSetIndex);
                for (var i = 0; i < _boundRunner.CurrentInputIndex && i < _row.Count; i++)
                {
                    SetTokenState(i, InputSetTokenState.Accepted);
                }

                _acceptedCount = _boundRunner.CurrentInputIndex;
                MarkCurrent(_acceptedCount);
            }
        }

        /// <summary>Stops presenting. A set still on screen (set collection timed out or aborted) leaves through its failed exit.</summary>
        public void Hide()
        {
            if (_boundRunner != null)
            {
                ReleaseRow(false);

                if (_boundRunner.TimedOut)
                {
                    PlayFeedback(setCollectionTimedOutFeedback, rowAnchor.position);
                }

                PlayFeedback(setCollectionEndedFeedback, rowAnchor.position);
            }

            Unbind();
        }

        private void Update()
        {
            if (_boundRunner == null || !_boundRunner.IsRunning)
            {
                return;
            }

            UpdateTimers(_boundRunner);
        }

        private void Unbind()
        {
            if (_boundRunner != null)
            {
                _boundRunner.SetStarted -= HandleSetStarted;
                _boundRunner.InputAccepted -= HandleInputAccepted;
                _boundRunner.SetFailed -= HandleSetFailed;
                _boundRunner.SetFinished -= HandleSetFinished;
                _boundRunner = null;
            }

            foreach (var view in _row)
            {
                // Null during scene teardown, when the pooled tokens may already be gone.
                if (view != null)
                {
                    view.gameObject.SetActive(false);
                }
            }

            _row.Clear();
            _rowSetIndex = -1;
            _acceptedCount = 0;
            SetTimersVisible(false);
            SetBackgroundVisible(false);
        }

        private void HandleSetStarted(int setIndex, int attempt)
        {
            if (attempt > 1 && setIndex == _rowSetIndex)
            {
                RetryRow();
                return;
            }

            // A finished set already handed its row off; this only clears a row the runner
            // left without finishing, which it never does today.
            ReleaseRow(false);
            BuildRow(setIndex);
            MarkCurrent(0);
            PlayFeedback(setStartedFeedback, rowAnchor.position);
        }

        private void HandleInputAccepted(int setIndex, int inputIndex)
        {
            if (setIndex != _rowSetIndex || inputIndex >= _row.Count)
            {
                return;
            }

            var view = _row[inputIndex];
            SetTokenState(inputIndex, InputSetTokenState.Accepted);
            view.PlayAccepted();
            PlayFeedback(inputAcceptedFeedback, view.transform.position);

            _acceptedCount = inputIndex + 1;
            MarkCurrent(_acceptedCount);
        }

        private void HandleSetFailed(InputSetFailure failure)
        {
            if (failure.SetIndex != _rowSetIndex)
            {
                return;
            }

            if (failure.Reason == InputSetFailureReason.SetTimedOut)
            {
                PlayFeedback(setTimedOutFeedback, rowAnchor.position);
                return;
            }

            var position = rowAnchor.position;
            if (failure.InputIndex < _row.Count)
            {
                var view = _row[failure.InputIndex];
                view.PlayWrong();
                position = view.transform.position;
            }

            PlayFeedback(wrongInputFeedback, position);
        }

        private void HandleSetFinished(InputSetResult result)
        {
            if (result.SetIndex != _rowSetIndex)
            {
                return;
            }

            var cleared = result.Outcome == InputSetOutcome.Cleared;

            // Handed off here rather than on the next SetStarted: clearing the last set raises
            // Completed (and so Hide) in the same call, which would cut the exit feedbacks short.
            ReleaseRow(cleared);

            if (cleared)
            {
                PlayFeedback(setClearedFeedback, rowAnchor.position);
                if (result.ClearedFirstTry)
                {
                    PlayFeedback(setClearedFirstTryFeedback, rowAnchor.position);
                }
            }
            else
            {
                PlayFeedback(setExhaustedFeedback, rowAnchor.position);
            }
        }

        private void BuildRow(int setIndex)
        {
            _rowSetIndex = setIndex;
            _acceptedCount = 0;

            var sets = _boundRunner.Definition.Sets;
            var inputs = setIndex >= 0 && setIndex < sets.Length ? sets[setIndex].inputs : null;
            if (inputs == null)
            {
                return;
            }

            var center = rowAnchor.position;
            var axis = rowAnchor.right;
            var halfSpan = (inputs.Length - 1) * 0.5f;

            for (var i = 0; i < inputs.Length; i++)
            {
                var pooled = tokenPool.GetPooledGameObject();
                if (pooled == null)
                {
                    Debug.LogWarning($"{nameof(InputSetVisualDriver)} on '{name}': token pool is exhausted — set {setIndex} shows only {i}/{inputs.Length} tokens. Raise the pool size or let it expand.", this);
                    return;
                }

                pooled.SetActive(true);
                var view = pooled.GetComponent<InputSetTokenView>();
                var sprite = IconFor(inputs[i], out var rotation);
                view.Place(center + axis * ((i - halfSpan) * spec.TokenSpacing), sprite, rotation);
                view.SetState(InputSetTokenState.Pending, spec.ColorFor(InputSetTokenState.Pending));
                _row.Add(view);
            }
        }

        private void RetryRow()
        {
            for (var i = 0; i < _row.Count; i++)
            {
                if (i < _acceptedCount)
                {
                    _row[i].PlayReset();
                }

                SetTokenState(i, InputSetTokenState.Pending);
            }

            _acceptedCount = 0;
            MarkCurrent(0);
            PlayFeedback(setRetriedFeedback, rowAnchor.position);
        }

        /// <summary>Hands every token of the row to its exit feedback, which returns it to the pool.</summary>
        private void ReleaseRow(bool cleared)
        {
            foreach (var view in _row)
            {
                if (view == null)
                {
                    continue;
                }

                if (cleared)
                {
                    view.PlayClearedExit();
                }
                else
                {
                    view.PlayFailedExit();
                }
            }

            _row.Clear();
            _rowSetIndex = -1;
            _acceptedCount = 0;
        }

        private void MarkCurrent(int inputIndex)
        {
            if (inputIndex < _row.Count)
            {
                SetTokenState(inputIndex, InputSetTokenState.Current);
            }
        }

        private void SetTokenState(int inputIndex, InputSetTokenState state)
        {
            _row[inputIndex].SetState(state, spec.ColorFor(state));
        }

        private void UpdateTimers(InputSetCollectionRunner runner)
        {
            if (setTimer != null)
            {
                var sets = runner.Definition.Sets;
                var setIndex = runner.CurrentSetIndex;
                var timeLimit = setIndex < sets.Length ? sets[setIndex].timeLimit : 0f;
                UpdateTimer(setTimer, timeLimit, runner.CurrentSetTimeRemaining);
            }

            if (setCollectionTimer != null)
            {
                var timeLimit = runner.Definition.TotalTimeLimit;
                UpdateTimer(setCollectionTimer, timeLimit, timeLimit - runner.ElapsedTime);
            }
        }

        private void UpdateTimer(InputSetTimerView timer, float timeLimit, float remaining)
        {
            var hasLimit = timeLimit > 0f;
            timer.SetVisible(hasLimit);
            if (!hasLimit)
            {
                return;
            }

            var normalized = Mathf.Clamp01(remaining / timeLimit);
            timer.SetFill(normalized, spec.TimerColorAt(normalized));
        }

        private void SetTimersVisible(bool visible)
        {
            if (setTimer != null)
            {
                setTimer.SetVisible(visible);
            }

            if (setCollectionTimer != null)
            {
                setCollectionTimer.SetVisible(visible);
            }
        }

        private void SetBackgroundVisible(bool visible)
        {
            if (background != null)
            {
                background.SetActive(visible);
            }
        }

        private Sprite IconFor(InputToken token, out Quaternion rotation)
        {
            if (!spec.TryGetIcon(token, out var sprite, out rotation) && _warnedUnmappedTokenIds.Add(token.Id))
            {
                Debug.LogWarning($"{nameof(InputSetVisualDriver)} on '{name}': {token} has no icon in the visual spec — its tokens keep the prefab's sprite.", this);
            }

            return sprite;
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
