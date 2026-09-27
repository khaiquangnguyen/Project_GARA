using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // Mermaid passive: her songs charm opponents (see AllNotesHitForCharm).
    // A fully charmed opponent fights for her side, under her side's
    // control, for a few of its own turns; its charm then starts over.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Superstar Singer", fileName = "SuperstarSingerPassive")]
    public class SuperstarSingerPassive : PassiveDefinition<SuperstarSingerState>
    {
        [Tooltip("Charm that fully charms an opponent.")]
        [Min(1)]
        [SerializeField] private int charmCapacity = 100;

        [Tooltip("Turns (the charmed character's own) it stays charmed.")]
        [Min(1)]
        [SerializeField] private int charmedTurns = 2;

        public int CharmCapacity => charmCapacity;

        // Adds charm from singer's song to target; no-op unless singer has
        // this passive and target is a living, uncharmed opponent.
        public static void AddCharm(ICombatTarget singer, ICombatTarget target, int amount)
        {
            var passives = singer?.Passives;
            if (passives == null || target == null || amount <= 0)
            {
                return;
            }

            if (target.IsDefeated || target.Faction == singer.Faction || target.HasStatus(StatusEffectKind.Charmed))
            {
                return;
            }

            foreach (var passive in passives.Passives)
            {
                if (passive is SuperstarSingerPassive superstar && passives.GetState(passive) is SuperstarSingerState state)
                {
                    superstar.Charm(singer, target, amount, state);
                    return;
                }
            }
        }

        private void Charm(ICombatTarget singer, ICombatTarget target, int amount, SuperstarSingerState state)
        {
            var charm = state.AddCharm(target, amount);
            Debug.Log($"[{nameof(SuperstarSingerPassive)}] charm {Mathf.Min(charm, charmCapacity)}/{charmCapacity}.");
            if (charm < charmCapacity)
            {
                return;
            }

            state.ClearCharm(target);
            target.ApplyStatus(StatusEffectInstance.Charm(singer.Faction, charmedTurns, passiveId));
        }

        protected override void OnSkillCardResolved(in PassiveContext context, SuperstarSingerState state)
        {
        }
    }
}
