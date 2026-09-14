using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GARA.Combat
{
    // Owns the "which participant is currently selected" cursor and the
    // placeholder indicator GameObject rendered at that participant's feet.
    // Selection is transient — opened once a single-target basic attack or
    // special is chosen, closed the instant it's confirmed or the player
    // switches to a different action — not something that spans a whole
    // Combat Phase. Candidates can be enemies or allies depending on what
    // was chosen; this class doesn't care which, it just cycles whatever
    // list it was given.
    public class TargetSelector : MonoBehaviour
    {
        [SerializeField] private GameObject targetIndicatorPrefab;

        private GameObject _indicatorInstance;
        private readonly List<CombatParticipant> _candidates = new();
        private int _currentIndex;

        public CombatParticipant CurrentTarget => _candidates.Count > 0 ? _candidates[_currentIndex] : null;
        public bool HasCandidates => _candidates.Count > 0;

        public void BeginSelection(IEnumerable<CombatParticipant> candidates)
        {
            _candidates.Clear();
            _candidates.AddRange(candidates.Where(c => !c.IsDefeated));
            _currentIndex = 0;

            if (_candidates.Count == 0)
            {
                HideIndicator();
                return;
            }

            ShowAt(CurrentTarget);
        }

        public void CycleLeft() => Cycle(-1);

        public void CycleRight() => Cycle(1);

        private void Cycle(int direction)
        {
            if (_candidates.Count == 0)
            {
                return;
            }

            _currentIndex = (_currentIndex + direction + _candidates.Count) % _candidates.Count;
            ShowAt(CurrentTarget);
        }

        public void EndSelection()
        {
            HideIndicator();
            _candidates.Clear();
        }

        // Drops a candidate that's no longer selectable (defeated mid-
        // selection, e.g. killed by an earlier swing in the same chain) —
        // a no-op if it isn't currently in the pool. Keeps the cursor on
        // whichever candidate it was pointing at when possible, only
        // shifting it back if that would run past the end of the list.
        public void RemoveCandidate(CombatParticipant participant)
        {
            var index = _candidates.IndexOf(participant);
            if (index < 0)
            {
                return;
            }

            _candidates.RemoveAt(index);

            if (_candidates.Count == 0)
            {
                _currentIndex = 0;
                HideIndicator();
                return;
            }

            if (_currentIndex >= _candidates.Count)
            {
                _currentIndex = _candidates.Count - 1;
            }

            ShowAt(CurrentTarget);
        }

        private void ShowAt(CombatParticipant target)
        {
            if (target == null)
            {
                HideIndicator();
                return;
            }

            if (_indicatorInstance == null)
            {
                _indicatorInstance = Instantiate(targetIndicatorPrefab);
            }

            _indicatorInstance.SetActive(true);
            _indicatorInstance.transform.position = target.SceneTransform.position;
        }

        private void HideIndicator()
        {
            if (_indicatorInstance != null)
            {
                _indicatorInstance.SetActive(false);
            }
        }
    }
}
