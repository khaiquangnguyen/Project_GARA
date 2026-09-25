using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.ShakeBalance
{
    /// <summary>
    /// Drives the on-screen presentation of a <see cref="ShakeBalanceRunner"/>: moves a needle along
    /// the track to the balance value, tints it by zone, sizes the perfect/good zone bands, tilts an
    /// optional target (a pan, a plate stack), and fills the result and time-left bars. Plays an
    /// optional <see cref="MMF_Player"/> at each key moment. Purely visual — all judging stays in
    /// <see cref="ShakeBalanceRunner"/>; this only reads it. Lives in the scene, not on a character:
    /// whoever owns the scene's visuals calls <see cref="Show"/> and <see cref="Hide"/> as runs
    /// start and end.
    /// </summary>
    public class ShakeBalanceVisualDriver : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private ShakeBalanceVisualSpec spec;

        [Tooltip("Center of the track; the needle moves along its right axis, Track Half Width to either side.")]
        [SerializeField]
        private Transform track;

        [Tooltip("Moved to the balance value each frame.")]
        [SerializeField]
        private Transform needle;

        [Tooltip("Optional. Tinted with the spec's color for the current zone. Defaults to the needle's SpriteRenderer.")]
        [SerializeField]
        private SpriteRenderer needleRenderer;

        [Tooltip("Optional. Scaled along local X to the perfect zone's width. Author it at full-track width (value -1 to +1).")]
        [SerializeField]
        private Transform perfectZoneBand;

        [Tooltip("Optional. Scaled along local X to the good zone's width. Author it at full-track width (value -1 to +1).")]
        [SerializeField]
        private Transform goodZoneBand;

        [Tooltip("Optional. Rotated with the balance value (see the spec's Max Tilt Degrees).")]
        [SerializeField]
        private Transform tiltTarget;

        [Tooltip("Optional. What cashing out right now would keep (0-1). Turn off its Hide Bar At Zero and Bump Scale On Change — it's updated every frame and starts empty.")]
        [SerializeField]
        private MMHealthBar resultBar;

        [Tooltip("Optional. Time left in the run; hidden when the run has no duration. Turn off its Hide Bar At Zero and Bump Scale On Change.")]
        [SerializeField]
        private MMHealthBar timerBar;

        [Tooltip("Optional. Shown only while a run is being presented — enabled by Show, disabled by Hide. Put the track, needle and bars under it; keep the feedbacks outside it so the end feedbacks aren't cut short by Hide.")]
        [SerializeField]
        private GameObject content;

        [Header("Feedbacks (optional)")]
        [Tooltip("Played at the track when a run starts showing.")]
        [SerializeField]
        private MMF_Player balanceStartedFeedback;

        [Tooltip("Played at the track when a run ends, however it ended.")]
        [SerializeField]
        private MMF_Player balanceEndedFeedback;

        [Tooltip("Played at the needle on each left push.")]
        [SerializeField]
        private MMF_Player pushedLeftFeedback;

        [Tooltip("Played at the needle on each right push.")]
        [SerializeField]
        private MMF_Player pushedRightFeedback;

        [Tooltip("Played at the needle when it enters the perfect zone.")]
        [SerializeField]
        private MMF_Player enteredPerfectFeedback;

        [Tooltip("Played at the needle when it enters the good zone (from either side).")]
        [SerializeField]
        private MMF_Player enteredGoodFeedback;

        [Tooltip("Played at the needle when it leaves the good zone — a warning.")]
        [SerializeField]
        private MMF_Player enteredOffFeedback;

        [Tooltip("Played at the track when the player cashes out, in addition to Balance Ended.")]
        [SerializeField]
        private MMF_Player cashedOutFeedback;

        [Tooltip("Played at the needle when the value falls off an edge, in addition to Balance Ended.")]
        [SerializeField]
        private MMF_Player fellFeedback;

        [Tooltip("Played at the track when the run's duration runs out, in addition to Balance Ended.")]
        [SerializeField]
        private MMF_Player timeUpFeedback;

        private ShakeBalanceRunner _boundRunner;
        private Vector3 _perfectBandScale;
        private Vector3 _goodBandScale;
        private Quaternion _tiltBaseRotation;

        private void Awake()
        {
            if (spec == null)
            {
                throw new InvalidOperationException($"{nameof(ShakeBalanceVisualDriver)} on '{name}' has no visual spec assigned.");
            }

            if (track == null)
            {
                throw new InvalidOperationException($"{nameof(ShakeBalanceVisualDriver)} on '{name}' has no track assigned.");
            }

            if (needle == null)
            {
                throw new InvalidOperationException($"{nameof(ShakeBalanceVisualDriver)} on '{name}' has no needle assigned.");
            }

            if (needleRenderer == null)
            {
                needleRenderer = needle.GetComponent<SpriteRenderer>();
            }

            if (perfectZoneBand != null)
            {
                _perfectBandScale = perfectZoneBand.localScale;
            }

            if (goodZoneBand != null)
            {
                _goodBandScale = goodZoneBand.localScale;
            }

            if (tiltTarget != null)
            {
                _tiltBaseRotation = tiltTarget.localRotation;
            }

            SetVisible(false);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// Starts presenting <paramref name="runner"/>, replacing whatever was shown before. Call it
        /// before or after the runner's Start() — the view follows the runner's live state either way.
        /// </summary>
        public void Show(ShakeBalanceRunner runner)
        {
            Unbind();

            if (runner == null)
            {
                return;
            }

            _boundRunner = runner;
            _boundRunner.Pushed += HandlePushed;
            _boundRunner.ZoneChanged += HandleZoneChanged;
            _boundRunner.Completed += HandleCompleted;

            SetVisible(true);
            SizeZoneBands(runner.Definition);
            if (timerBar != null)
            {
                timerBar.gameObject.SetActive(runner.Definition.Duration > 0f);
            }

            Refresh(runner);
            PlayFeedback(balanceStartedFeedback, track.position);
        }

        /// <summary>Stops presenting.</summary>
        public void Hide()
        {
            Unbind();
        }

        private void Update()
        {
            if (_boundRunner == null || !_boundRunner.IsRunning)
            {
                return;
            }

            Refresh(_boundRunner);
        }

        private void Unbind()
        {
            if (_boundRunner != null)
            {
                _boundRunner.Pushed -= HandlePushed;
                _boundRunner.ZoneChanged -= HandleZoneChanged;
                _boundRunner.Completed -= HandleCompleted;
                _boundRunner = null;
            }

            SetVisible(false);
        }

        private void Refresh(ShakeBalanceRunner runner)
        {
            var value = runner.Value;
            needle.position = track.position + track.right * (value * spec.TrackHalfWidth);

            if (needleRenderer != null)
            {
                needleRenderer.color = spec.ColorFor(runner.Zone);
            }

            if (tiltTarget != null)
            {
                tiltTarget.localRotation = _tiltBaseRotation * spec.TiltFor(value);
            }

            if (resultBar != null)
            {
                resultBar.UpdateBar(runner.CurrentResult, 0f, 1f, true);
            }

            var duration = runner.Definition.Duration;
            if (timerBar != null && duration > 0f)
            {
                timerBar.UpdateBar(runner.TimeRemaining, 0f, duration, true);
            }
        }

        private void HandlePushed(int direction)
        {
            PlayFeedback(direction < 0 ? pushedLeftFeedback : pushedRightFeedback, needle.position);
        }

        private void HandleZoneChanged(ShakeBalanceZone zone)
        {
            switch (zone)
            {
                case ShakeBalanceZone.Perfect:
                    PlayFeedback(enteredPerfectFeedback, needle.position);
                    break;
                case ShakeBalanceZone.Good:
                    PlayFeedback(enteredGoodFeedback, needle.position);
                    break;
                default:
                    PlayFeedback(enteredOffFeedback, needle.position);
                    break;
            }
        }

        private void HandleCompleted(ShakeBalanceReport report)
        {
            // Already hidden: whoever owns the run handled Completed first. Owners subscribe after
            // Show so this doesn't happen, but a late subscriber shouldn't break the driver.
            if (_boundRunner == null)
            {
                return;
            }

            // The last frame's state (e.g. the needle pinned to the edge it fell off), shown before
            // the end feedbacks play. Hide follows from whoever owns the run.
            Refresh(_boundRunner);

            if (resultBar != null)
            {
                resultBar.UpdateBar(report.Result, 0f, 1f, true);
            }

            switch (report.EndReason)
            {
                case ShakeBalanceEndReason.CashedOut:
                    PlayFeedback(cashedOutFeedback, track.position);
                    break;
                case ShakeBalanceEndReason.Fell:
                    PlayFeedback(fellFeedback, needle.position);
                    break;
                case ShakeBalanceEndReason.TimeUp:
                    PlayFeedback(timeUpFeedback, track.position);
                    break;
            }

            PlayFeedback(balanceEndedFeedback, track.position);
        }

        private void SizeZoneBands(ShakeBalanceDefinition definition)
        {
            if (perfectZoneBand != null)
            {
                perfectZoneBand.position = track.position;
                perfectZoneBand.localScale = new Vector3(_perfectBandScale.x * definition.PerfectZone, _perfectBandScale.y, _perfectBandScale.z);
            }

            if (goodZoneBand != null)
            {
                goodZoneBand.position = track.position;
                goodZoneBand.localScale = new Vector3(_goodBandScale.x * definition.GoodZone, _goodBandScale.y, _goodBandScale.z);
            }
        }

        private void SetVisible(bool visible)
        {
            if (content != null)
            {
                content.SetActive(visible);
            }

            if (timerBar != null && !visible)
            {
                timerBar.gameObject.SetActive(false);
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
