using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // Mermaid passive: every contact with an opponent — her hits, its hits on
    // her, her parries — charms it. A fully charmed opponent fights for her
    // side, under her side's control, for a few of its own turns.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Superstar Singer", fileName = "SuperstarSingerPassive")]
    public class SuperstarSingerPassive : PassiveDefinition<SuperstarSingerState>
    {
        [Tooltip("Charm that fully charms an opponent.")]
        [Min(1)]
        [SerializeField] private int charmCapacity = 100;

        [Tooltip("Turns (the charmed character's own) it stays charmed.")]
        [Min(1)]
        [SerializeField] private int charmedTurns = 2;

        [Header("Contact charm")]
        [Min(0)]
        [SerializeField] private int charmPerHitDealt = 10;

        [Min(0)]
        [SerializeField] private int charmPerHitTaken = 10;

        [Min(0)]
        [SerializeField] private int charmPerParry = 20;

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

            foreach (var passive in passives.Passives)
            {
                if (passive is SuperstarSingerPassive superstar && passives.GetState(passive) is SuperstarSingerState state)
                {
                    superstar.TryCharm(singer, target, amount, state);
                    return;
                }
            }
        }

        private void TryCharm(ICombatTarget singer, ICombatTarget target, int amount, SuperstarSingerState state)
        {
            if (amount <= 0 || target.IsDefeated || target.Faction == singer.Faction || target.HasStatus(StatusEffectKind.Charmed))
            {
                return;
            }

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

        protected override void OnHitLanded(ICombatTarget self, ICombatTarget target, int damage, SuperstarSingerState state)
        {
            TryCharm(self, target, charmPerHitDealt, state);
        }

        protected override void OnHitTaken(ICombatTarget self, ICombatTarget attacker, int damage, SuperstarSingerState state)
        {
            TryCharm(self, attacker, charmPerHitTaken, state);
        }

        protected override void OnParried(ICombatTarget self, ICombatTarget attacker, SuperstarSingerState state)
        {
            TryCharm(self, attacker, charmPerParry, state);
        }
    }
}
