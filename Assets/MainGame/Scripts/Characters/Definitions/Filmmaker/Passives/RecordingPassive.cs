using System.Collections.Generic;
using UnityEngine;

namespace GARA.Characters.Filmmaker
{
    // Filmmaker passive: each special hit records its targets. A fully
    // recorded opponent hands the Filmmaker a one-time replay of one of its
    // skills (the last it used, else a random one), performed in its form
    // (see FormReplaySkillCard); its recording then starts over.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Recording", fileName = "RecordingPassive")]
    public class RecordingPassive : PassiveDefinition<RecordingState>
    {
        [Tooltip("Stacks that fully record an opponent.")]
        [Min(1)]
        [SerializeField] private int fullStacks = 3;

        [Tooltip("Replays the Filmmaker can hold at once; a full recording waits while at the limit.")]
        [Min(1)]
        [SerializeField] private int maxHeldReplays = 1;

        [SerializeField] private FormReplayMode replayMode = FormReplayMode.StandIn;

        public int FullStacks => fullStacks;

        // No-op unless filmmaker has this passive and target is a living
        // opponent.
        public static void Record(ICombatTarget filmmaker, ICombatTarget target, int stacks)
        {
            var passives = filmmaker?.Passives;
            if (passives == null || target == null || stacks <= 0 || target.IsDefeated || target.Faction == filmmaker.Faction)
            {
                return;
            }

            foreach (var passive in passives.Passives)
            {
                if (passive is RecordingPassive recording && passives.GetState(passive) is RecordingState state)
                {
                    recording.Record(filmmaker, target, stacks, state);
                    return;
                }
            }
        }

        private void Record(ICombatTarget filmmaker, ICombatTarget target, int stacks, RecordingState state)
        {
            var recorded = Mathf.Min(state.StacksOn(target) + stacks, fullStacks);
            state.SetStacks(target, recorded);
            Debug.Log($"[{nameof(RecordingPassive)}] recording {recorded}/{fullStacks} on {target.Definition?.displayName}.");
            if (recorded < fullStacks || HeldReplays(filmmaker) >= maxHeldReplays)
            {
                return;
            }

            var replay = FormReplaySkillCard.Create(PickSkill(target), target.Definition, replayMode, oneTimeUse: true);
            if (replay == null)
            {
                return;
            }

            filmmaker.AddSkillCard(replay);
            state.SetStacks(target, 0);
            Debug.Log($"[{nameof(RecordingPassive)}] {target.Definition.displayName} fully recorded — got {replay.displayName}.");
        }

        private static int HeldReplays(ICombatTarget filmmaker)
        {
            var held = 0;
            foreach (var card in filmmaker.SkillCards)
            {
                if (card is FormReplaySkillCard)
                {
                    held++;
                }
            }

            return held;
        }

        // Its last skill, else a random one; only skills its form can play.
        private static SkillCardDefinition PickSkill(ICombatTarget target)
        {
            var form = target.Definition;
            if (form == null)
            {
                return null;
            }

            var last = target.LastUsedSkillCard;
            if (last != null && form.FindSkillCardState(last) != null)
            {
                return last;
            }

            var candidates = new List<SkillCardDefinition>();
            foreach (var card in target.SkillCards)
            {
                if (card != null && !card.IsOneTimeUse && form.FindSkillCardState(card) != null)
                {
                    candidates.Add(card);
                }
            }

            return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
        }

        protected override void OnSkillCardResolved(in PassiveContext context, RecordingState state)
        {
        }
    }
}
