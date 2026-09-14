using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Transient battle-scoped wrapper. Created fresh at battle start, torn
    // down at battle end — nothing here persists except through the
    // post-battle reconciliation step (XP, promotion, etc.).
    public class CombatParticipant : ICombatTarget
    {
        public string participantId;

        // Exactly one of these is set depending on what this participant wraps.
        public ManagedCharacter source;
        public EnemyEncounterData enemySource;

        public CharacterDefinition definition;
        public FactionTag faction;

        public StatBlock baseCombatStats;
        public int currentHp;
        public int currentMp;
        public int currentAp;
        public int speed;

        public FormationSlot slot;

        public BasicAttackController basicAttackController;

        public GameObject SceneRoot { get; private set; }
        public Transform SceneTransform => SceneRoot != null ? SceneRoot.transform : null;

        private readonly Dictionary<CharacterState, CharacterState> _liveStateByAssetReference = new();

        public bool IsDefeated => currentHp <= 0;

        public static CombatParticipant FromManagedCharacter(ManagedCharacter character, CharacterDefinition definition, FactionTag faction)
        {
            // Snapshot: out-of-battle modifiers are baked into the baseline at
            // battle start, so later equipment changes can't reach this battle.
            var stats = character.GetEffectiveStats(definition).Snapshot();
            return new CombatParticipant
            {
                participantId = System.Guid.NewGuid().ToString(),
                source = character,
                definition = definition,
                faction = faction,
                baseCombatStats = stats,
                currentHp = stats.MaxHp.Value,
                currentMp = stats.MaxMp.Value,
                currentAp = stats.MaxAp.Value,
                speed = stats.Speed.Value,
                basicAttackController = new BasicAttackController(definition)
            };
        }

        public static CombatParticipant FromEnemyEncounter(EnemyEncounterData encounter, FactionTag faction)
        {
            var stats = encounter.definition.GetStatsAtLevel(encounter.level);
            return new CombatParticipant
            {
                participantId = System.Guid.NewGuid().ToString(),
                enemySource = encounter,
                definition = encounter.definition,
                faction = faction,
                baseCombatStats = stats,
                currentHp = stats.MaxHp.Value,
                currentMp = stats.MaxMp.Value,
                currentAp = stats.MaxAp.Value,
                speed = stats.Speed.Value,
                basicAttackController = new BasicAttackController(encounter.definition)
            };
        }

        // No status effect system yet — returns a copy of the baseline with no
        // modifiers. Kept as a method (rather than exposing BaseCombatStats
        // directly to callers) so callers don't need to change once statuses
        // exist, and so the battle-start snapshot is never mutated.
        public StatBlock GetCurrentStats()
        {
            return baseCombatStats.WithModifiers(null);
        }

        public void ApplyDamage(int amount)
        {
            currentHp = Mathf.Max(0, currentHp - amount);
        }

        public bool TrySpendAp(int amount)
        {
            if (amount < 0 || currentAp < amount)
            {
                return false;
            }

            currentAp -= amount;
            return true;
        }

        public bool TrySpendMp(int amount)
        {
            if (amount < 0 || currentMp < amount)
            {
                return false;
            }

            currentMp -= amount;
            return true;
        }

        // Atomic check-then-deduct-both — a special must never spend one
        // resource without affording the other.
        public bool TrySpendResources(int apCost, int mpCost)
        {
            if (currentAp < apCost || currentMp < mpCost)
            {
                return false;
            }

            currentAp -= apCost;
            currentMp -= mpCost;
            return true;
        }

        // Called once the character's prefab has been instantiated for this
        // battle. Separate from the factories above because party/roster
        // composition is data-only and can happen before any prefab exists —
        // this is also the one place live CharacterStates get resolved, so
        // a misauthored prefab (missing component) fails fast, once, here.
        public void BindToSceneInstance(GameObject instantiatedRoot)
        {
            SceneRoot = instantiatedRoot;
            ResolveCharacterStates(instantiatedRoot);
        }

        private void ResolveCharacterStates(GameObject instantiatedRoot)
        {
            foreach (var assetSideRef in definition.EnumerateCharacterStateReferences())
            {
                if (assetSideRef == null || _liveStateByAssetReference.ContainsKey(assetSideRef))
                {
                    continue;
                }

                var live = instantiatedRoot.GetComponentInChildren(assetSideRef.GetType(), true) as CharacterState;
                _liveStateByAssetReference[assetSideRef] = live;
            }
        }

        public CharacterState ResolveLiveState(CharacterState assetSideReference)
        {
            return _liveStateByAssetReference.TryGetValue(assetSideReference, out var live) ? live : null;
        }

        FactionTag ICombatTarget.Faction => faction;
        bool ICombatTarget.IsDefeated => IsDefeated;
        void ICombatTarget.ApplyDamage(int amount) => ApplyDamage(amount);
    }
}
