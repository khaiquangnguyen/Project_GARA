using System;
using GARA.Characters;
using GARA.Input;
using UnityEngine;

namespace GARA.Combat
{
    // Combat-side implementation of ISkillInputHost — gives a skill card's
    // input session access to whatever driver component its concrete
    // minigame needs (a rhythm conductor, a shake detector, etc.) plus a
    // MonoBehaviour to run coroutines on, without GARA.Characters needing to
    // know anything about GARA.Combat. Also raises the two UI-facing
    // announcements around an input phase (static events, invoked from an
    // instance method).
    public class SkillCardInputHost : MonoBehaviour, ISkillInputHost
    {
        [SerializeField] private Component[] drivers;

        public static event Action<SkillCardDefinition> InputPhaseStarted;
        public static event Action<SkillPerformance> InputPhaseEnded;

        public MonoBehaviour CoroutineRunner => this;
        public CharacterDefinition Actor { get; set; }
        public bool IsPlayerControlled { get; set; } = true;

        public T GetDriver<T>() where T : Component
        {
            foreach (var driver in drivers)
            {
                if (driver is T match)
                {
                    return match;
                }
            }

            return null;
        }

        // Sets every driver that can play itself (Character Test window).
        public void SetAutoPlay(AutoPlayMode mode)
        {
            foreach (var driver in drivers)
            {
                if (driver is IAutoPlayable autoPlayable)
                {
                    autoPlayable.AutoPlay = mode;
                }
            }
        }

        public void RaiseInputPhaseStarted(SkillCardDefinition card)
        {
            InputPhaseStarted?.Invoke(card);
        }

        public void RaiseInputPhaseEnded(SkillPerformance performance)
        {
            InputPhaseEnded?.Invoke(performance);
        }
    }
}
