using UnityEngine;

namespace GARA.Characters.Noir
{
    // Noir passive: Noir's cards stack clues on the opponents they hit. Enough
    // clues noirify that opponent. Every clue also fills the Case File;
    // filling it, or any noirified character being defeated, pulls everyone
    // into the Noir World.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Noir", fileName = "NoirPassive")]
    public class NoirPassive : PassiveDefinition<NoirState>
    {
        [Header("Clues (per opponent hit, by performance tier)")]
        [Min(0)]
        [SerializeField] private int cluesOnOk = 1;

        [Min(0)]
        [SerializeField] private int cluesOnGood = 1;

        [Min(0)]
        [SerializeField] private int cluesOnPerfect = 2;

        [Header("Noirification")]
        [Tooltip("Clues on one character that noirify it.")]
        [Min(1)]
        [SerializeField] private int noirifyThreshold = 3;

        [Header("Case File")]
        [Tooltip("Clues (from any opponent) that fill the Case File and open the Noir World.")]
        [Min(1)]
        [SerializeField] private int caseFileCapacity = 8;

        [Header("Noir World")]
        [Tooltip("Turns (anyone's) the Noir World lasts once entered.")]
        [Min(1)]
        [SerializeField] private int worldDurationTurns = 6;

        public int NoirifyThreshold => noirifyThreshold;
        public int CaseFileCapacity => caseFileCapacity;

        protected override void OnSkillCardResolved(in PassiveContext context, NoirState state)
        {
            if (context.Performance.WasAborted || context.CardTargets == null)
            {
                return;
            }

            var clues = CluesFor(context.Performance.Tier);
            if (clues <= 0)
            {
                return;
            }

            foreach (var target in context.CardTargets)
            {
                if (target == null || target.IsDefeated || target.Faction == context.Self.Faction)
                {
                    continue;
                }

                var stacked = state.AddClues(target, clues);
                if (stacked >= noirifyThreshold && !target.HasStatus(StatusEffectKind.Noirified))
                {
                    target.ApplyStatus(StatusEffectInstance.Permanent(StatusEffectKind.Noirified, passiveId));
                }

                // The case is already open while inside the world.
                if (!context.Battle.NoirWorld.IsActive)
                {
                    state.AddToCaseFile(clues);
                }
            }

            if (state.CaseFile >= caseFileCapacity)
            {
                EnterWorld(context, state);
            }
        }

        // A noirified body cracks the case open, whoever landed the blow.
        protected override void OnParticipantDefeated(in PassiveContext context, ICombatTarget defeated, NoirState state)
        {
            if (defeated.HasStatus(StatusEffectKind.Noirified))
            {
                EnterWorld(context, state);
            }
        }

        private void EnterWorld(in PassiveContext context, NoirState state)
        {
            if (context.Self.IsDefeated)
            {
                return;
            }

            if (context.Battle.NoirWorld.TryEnter(context.Self, worldDurationTurns))
            {
                state.CloseCaseFile();
            }
        }

        private int CluesFor(SkillPerformanceTier tier)
        {
            switch (tier)
            {
                case SkillPerformanceTier.Ok:
                    return cluesOnOk;
                case SkillPerformanceTier.Good:
                    return cluesOnGood;
                case SkillPerformanceTier.Perfect:
                    return cluesOnPerfect;
                default:
                    return 0;
            }
        }
    }
}
