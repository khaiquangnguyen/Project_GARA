using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;

namespace GARA.Combat
{
    // Owns the target cursor and the indicators rendered above the
    // head of whoever's selected. Opened once a skill card is chosen and
    // closed the instant it fires or is cancelled. Candidates can be enemies
    // or allies; this class just cycles whatever list it was given.
    public class TargetSelector : MonoBehaviour
    {
        [SerializeField] private GameObject targetIndicatorPrefab;

        private readonly List<GameObject> _indicators = new();
        private readonly List<CombatParticipant> _candidates = new();
        private readonly List<CombatParticipant> _marked = new();
        private int _currentIndex;
        private bool _selectsAll;

        public CombatParticipant CurrentTarget => _candidates.Count > 0 ? _candidates[_currentIndex] : null;
        public bool HasCandidates => _candidates.Count > 0;
        public IReadOnlyList<CombatParticipant> Candidates => _candidates;

        // Everyone an indicator is on: the cursor (or the whole pool when
        // selecting all) plus any marked picks.
        public IEnumerable<CombatParticipant> Selected
        {
            get
            {
                var cursor = _selectsAll || _candidates.Count == 0
                    ? _candidates
                    : new List<CombatParticipant> { CurrentTarget };
                return _marked.Concat(cursor).Distinct();
            }
        }

        // selectAll puts an indicator on every candidate and disables cycling.
        public void BeginSelection(IEnumerable<CombatParticipant> candidates, bool selectAll = false)
        {
            _candidates.Clear();
            _candidates.AddRange(candidates.Where(c => !c.IsDefeated));
            _marked.Clear();
            _currentIndex = 0;
            _selectsAll = selectAll;
            RefreshIndicators();
        }

        public void CycleLeft() => Cycle(-1);

        public void CycleRight() => Cycle(1);

        private void Cycle(int direction)
        {
            if (_candidates.Count == 0 || _selectsAll)
            {
                return;
            }

            _currentIndex = (_currentIndex + direction + _candidates.Count) % _candidates.Count;
            RefreshIndicators();
        }

        // Already-picked targets that keep an indicator while the cursor moves on.
        public void SetMarked(IEnumerable<CombatParticipant> marked)
        {
            _marked.Clear();
            _marked.AddRange(marked);
            RefreshIndicators();
        }

        public void EndSelection()
        {
            _candidates.Clear();
            _marked.Clear();
            _selectsAll = false;
            RefreshIndicators();
        }

        // Drops a candidate that's no longer selectable (defeated) — a no-op
        // if it isn't in the pool. Keeps the cursor on its target if possible.
        public void RemoveCandidate(CombatParticipant participant)
        {
            _marked.Remove(participant);
            var index = _candidates.IndexOf(participant);
            if (index < 0)
            {
                RefreshIndicators();
                return;
            }

            _candidates.RemoveAt(index);
            if (index < _currentIndex || _currentIndex >= _candidates.Count)
            {
                _currentIndex = Mathf.Max(0, _currentIndex - 1);
            }

            RefreshIndicators();
        }

        private void RefreshIndicators()
        {
            var selected = Selected.Where(target => target != null).ToList();
            while (_indicators.Count < selected.Count)
            {
                _indicators.Add(Instantiate(targetIndicatorPrefab));
            }

            for (var i = 0; i < _indicators.Count; i++)
            {
                var show = i < selected.Count;
                _indicators[i].SetActive(show);
                if (show)
                {
                    _indicators[i].transform.position = TopOf(selected[i]);
                }
            }
        }

        // Top-centre of the participant's Spine mesh, or its root if it has none.
        private static Vector3 TopOf(CombatParticipant participant)
        {
            var skeleton = participant.SceneRoot.GetComponentInChildren<SkeletonRenderer>();
            if (skeleton == null || !skeleton.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                return participant.SceneTransform.position;
            }

            var bounds = meshRenderer.bounds;
            return new Vector3(bounds.center.x, bounds.max.y, participant.SceneTransform.position.z);
        }
    }
}
