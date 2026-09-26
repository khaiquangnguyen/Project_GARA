using System;
using GARA.Characters;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace GARA.Combat
{
    // Scene-side presentation and turn ticking of the battle's Noir World
    // (see NoirWorld). What the world does to characters comes later.
    public partial class CombatSceneManager
    {
        public static event Action<NoirWorld> NoirWorldEntered;
        public static event Action<NoirWorld> NoirWorldExited;

        [Header("Noir World")]
        [Tooltip("Played as everyone is pulled into the Noir World (greyscale, rain, music swap, ...).")]
        [SerializeField] private MMF_Player noirWorldEnterFeedback;

        [Tooltip("Played as the Noir World ends.")]
        [SerializeField] private MMF_Player noirWorldExitFeedback;

        private NoirWorld _noirWorld;

        private void BindNoirWorld(NoirWorld world)
        {
            UnbindNoirWorld();
            _noirWorld = world;
            _noirWorld.Entered += OnNoirWorldEntered;
            _noirWorld.Exited += OnNoirWorldExited;
        }

        private void UnbindNoirWorld()
        {
            if (_noirWorld == null)
            {
                return;
            }

            _noirWorld.Entered -= OnNoirWorldEntered;
            _noirWorld.Exited -= OnNoirWorldExited;
            _noirWorld = null;
        }

        private void OnNoirWorldEntered(NoirWorld world)
        {
            if (noirWorldEnterFeedback != null)
            {
                noirWorldEnterFeedback.PlayFeedbacks();
            }

            NoirWorldEntered?.Invoke(world);
        }

        private void OnNoirWorldExited(NoirWorld world)
        {
            if (noirWorldExitFeedback != null)
            {
                noirWorldExitFeedback.PlayFeedbacks();
            }

            NoirWorldExited?.Invoke(world);
        }

        // Every standing participant's passives hear about each defeat.
        private void NotifyPassivesOfDefeat(CombatParticipant defeated)
        {
            foreach (var participant in _battle.AllParticipants)
            {
                if (participant == defeated || participant.IsDefeated)
                {
                    continue;
                }

                var context = new PassiveContext(_battle.QueryFor(participant), participant, null, null, default);
                participant.Passives.NotifyParticipantDefeated(in context, defeated);
            }
        }
    }
}
