using System;
using System.Collections.Generic;
using GARA.Characters;
using MoreMountains.Tools;
using NaughtyAttributes;
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
        [Expandable]
        public EnemyEncounterData enemySource;

        public CharacterDefinition definition;
        public FactionTag faction;

        public StatBlock baseCombatStats;
        public int currentHp;
        public int currentMp;
        public int currentAp;
        public int speed;

        public FormationSlot slot;

        public GameObject SceneRoot { get; private set; }
        public Transform SceneTransform => SceneRoot != null ? SceneRoot.transform : null;

        private readonly Dictionary<CharacterState, CharacterState> _liveStateByAssetReference = new();

        // Active buffs/debuffs from resolved skill effects (see
        // TimedStatModifierSkillEffect). Plugs into the same
        // StatBlock.WithModifiers pipeline equipment/passives use — see
        // GetCurrentStats.
        private readonly List<TimedStatModifier> _timedModifiers = new();

        // Class-passive runtime state, built lazily from the definition's
        // authored Passives the first time anything asks. Lives here (not
        // on CharacterDefinition) so mutable per-battle state never touches
        // the shared asset.
        private PassiveRuntimeSet _passives;

        public PassiveRuntimeSet Passives => _passives ??= new PassiveRuntimeSet(definition != null ? definition.Passives : null);

        // The skill cards brought into this battle — resolved once at battle
        // start from the definition's pool (see
        // CharacterDefinition.ResolveCombatLoadout). Everything in combat
        // picks cards from here, never from the definition's full pool.
        private List<SkillCardDefinition> _skillCards = new();

        public IReadOnlyList<SkillCardDefinition> SkillCards => _skillCards;

        // Active statuses (e.g. food coma) inflicted by cooking/other skill
        // effects. Mirrors _timedModifiers' shape but kept separate — a
        // status shapes incoming damage/turn-skipping rather than StatBlock.
        private readonly List<StatusEffectInstance> _statuses = new();

        // Cooking (Chef) fullness meter — see Feed. Never negative; wraps
        // back down (with overflow carried) once it crosses the palate's
        // capacity, rather than resetting to 0.
        private int _fullness;

        public bool IsDefeated => currentHp <= 0;

        // Raised the instant a participant's HP crosses into defeated —
        // once, on that transition only, not on every subsequent hit. Lets
        // combat-flow code (CombatSceneManager) react without
        // CombatParticipant needing to know about TargetSelector/turn order
        // itself.
        public static event Action<CombatParticipant> Defeated;

        // Raised after every change to currentHp, so HP displays can follow.
        public static event Action<CombatParticipant> HpChanged;

        public static CombatParticipant FromManagedCharacter(ManagedCharacter character, CharacterDefinition definition, FactionTag faction)
        {
            // Snapshot: out-of-battle modifiers are baked into the baseline at
            // battle start, so later equipment changes can't reach this battle.
            var stats = character.GetEffectiveStats(definition).Snapshot();
            var participant = new CombatParticipant
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
                _skillCards = definition.ResolveCombatLoadout(character.equippedSkillCardIds)
            };
            return participant;
        }

        public static CombatParticipant FromEnemyEncounter(EnemyEncounterData encounter, FactionTag faction)
        {
            var stats = encounter.definition.GetStatsAtLevel(encounter.level);
            var participant = new CombatParticipant
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
                _skillCards = encounter.definition.ResolveCombatLoadout()
            };
            return participant;
        }

        public bool TryGetSkillCard(int index, out SkillCardDefinition card)
        {
            if (index >= 0 && index < _skillCards.Count)
            {
                card = _skillCards[index];
                return true;
            }

            card = null;
            return false;
        }

        // Returns a copy of the baseline with every active timed modifier
        // (see ApplyTimedModifier) applied. Kept as a method (rather than
        // exposing BaseCombatStats directly to callers) so the battle-start
        // snapshot is never mutated. CurrentStats (ICombatTarget) delegates
        // here so both surfaces stay in sync.
        public StatBlock GetCurrentStats()
        {
            return baseCombatStats.WithModifiers(_timedModifiers);
        }

        // Adds a new timed buff/debuff — see TimedStatModifierSkillEffect.
        public void ApplyTimedModifier(TimedStatModifier modifier)
        {
            _timedModifiers.Add(modifier);
        }

        // Ticks every active timed modifier down by one turn and drops any
        // that have expired. Called once at the start of THIS participant's
        // own phase (see CombatSceneManager.BeginPhaseForCurrentActor) —
        // not on every global turn — so a modifier's authored duration means
        // "lasts N of the owner's own turns."
        public void TickTimedModifiers()
        {
            for (var i = _timedModifiers.Count - 1; i >= 0; i--)
            {
                _timedModifiers[i].remainingTurns--;
                if (_timedModifiers[i].remainingTurns <= 0)
                {
                    _timedModifiers.RemoveAt(i);
                }
            }
        }

        // Parry/jump windows use scaled time, so a freeze frame pauses them.
        public bool IsParrying => Time.time < _parryEndsAt;

        private float _parryEndsAt = float.NegativeInfinity;

        public void BeginParry(float durationSeconds)
        {
            _parryEndsAt = Time.time + durationSeconds;
        }

        public static event Action<CombatParticipant> ParrySucceeded;

        public bool IsJumping => Time.time < _jumpEndsAt;

        private float _jumpEndsAt = float.NegativeInfinity;

        public void BeginJump(float durationSeconds)
        {
            _jumpEndsAt = Time.time + durationSeconds;
        }

        // A hit inside a parry or jump window is negated (parry wins if both
        // are open). A hit on an already-defeated character is ignored
        // outright — no parry/jump success, no hit effect or reaction.
        public void ApplyDamage(int amount)
        {
            if (IsDefeated)
            {
                return;
            }

            if (IsParrying)
            {
                MMEventManager.TriggerEvent(new ParrySuccessStateEvent(SceneRoot));
                ParrySucceeded?.Invoke(this);
                return;
            }

            if (IsJumping)
            {
                MMEventManager.TriggerEvent(new JumpSuccessStateEvent(SceneRoot));
                return;
            }

            ApplyDamageCore(ModifyIncomingDamage(amount));
        }

        // Applies every active status's incoming-damage shaping: multipliers
        // first (multiplicative, order-independent), then flat per-hit bonuses.
        // Never reduces a hit below 0. Generic on purpose — a future Vulnerable
        // or Guarded status plugs in with no further edits here.
        private int ModifyIncomingDamage(int amount)
        {
            if (amount <= 0)
            {
                return amount;
            }

            var result = (float)amount;
            var flatBonus = 0;
            foreach (var status in _statuses)
            {
                if (status.IsExpired)
                {
                    continue;
                }

                result *= status.incomingDamageMultiplier;
                flatBonus += status.bonusDamageTakenPerHit;
            }

            return Mathf.Max(0, Mathf.RoundToInt(result) + flatBonus);
        }

        // Shared HP/death/event logic between a normal hit (ApplyDamage,
        // which shapes the amount through ModifyIncomingDamage first) and a
        // status's own per-turn tick damage (ApplyStatusTickDamage, which
        // must NOT be re-shaped by the very status inflicting it).
        // Already-defeated characters take no further damage, so the death
        // reaction and Defeated fire exactly once, on the killing blow.
        private void ApplyDamageCore(int amount)
        {
            if (IsDefeated)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - amount);
            HpChanged?.Invoke(this);
            MMEventManager.TriggerEvent(new HitStateEvent(SceneRoot));

            var executor = SceneRoot.GetComponent<AttackExecutor>();
            if (IsDefeated)
            {
                executor?.PlayDeathReaction();
                Defeated?.Invoke(this);
            }
            else
            {
                executor?.PlayHitReaction();
            }
        }

        // Cooking (Chef): feeds this participant, returning how much
        // fullness was gained and whether it tipped over into a food coma.
        // A no-op (NotFeedable) for anything that can't be fed, is already
        // defeated, or a non-positive amount.
        public PalateProfile Palate => definition != null ? definition.palate : PalateProfile.None;

        public int Fullness => _fullness;

        public bool IsStunned
        {
            get
            {
                foreach (var status in _statuses)
                {
                    if (status.skipsTurn && !status.IsExpired)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public int PendingSkippedTurnDamage
        {
            get
            {
                var total = 0;
                foreach (var status in _statuses)
                {
                    if (status.skipsTurn && !status.IsExpired)
                    {
                        total += status.damagePerSkippedTurn;
                    }
                }

                return total;
            }
        }

        public static event Action<CombatParticipant> FullnessChanged;
        public static event Action<CombatParticipant> BecameFull;

        public FeedResult Feed(int amount)
        {
            if (!Palate.CanBeFed || IsDefeated || amount <= 0)
            {
                return FeedResult.NotFeedable;
            }

            _fullness += amount;
            var capacity = Palate.FullnessCapacity;
            var becameFull = _fullness >= capacity;
            var overflow = becameFull ? _fullness - capacity : 0;
            if (becameFull)
            {
                // Carry the remainder rather than zeroing out, clamped so one
                // huge feeding can never chain a second coma trigger on the
                // same feed.
                _fullness = Mathf.Clamp(overflow, 0, capacity - 1);
            }

            var result = new FeedResult(amount, _fullness, capacity, becameFull, overflow);
            FullnessChanged?.Invoke(this);
            if (becameFull)
            {
                BecameFull?.Invoke(this);
            }

            return result;
        }

        public void ApplyStatus(StatusEffectInstance status)
        {
            _statuses.Add(status);
        }

        // Ticks every NON-skip status down by one turn (same "N of the owner's
        // own turns" semantics as TickTimedModifiers) and drops expired
        // entries. Skip statuses are ticked separately by ConsumeSkippedTurn,
        // from inside
        // the turn-skip itself, so the count of skipped turns is exactly the
        // authored duration.
        public void TickStatuses()
        {
            for (var i = _statuses.Count - 1; i >= 0; i--)
            {
                if (_statuses[i].skipsTurn)
                {
                    continue;
                }

                _statuses[i].remainingTurns--;
                if (_statuses[i].IsExpired)
                {
                    _statuses.RemoveAt(i);
                }
            }
        }

        public void ConsumeSkippedTurn()
        {
            for (var i = _statuses.Count - 1; i >= 0; i--)
            {
                if (!_statuses[i].skipsTurn)
                {
                    continue;
                }

                _statuses[i].remainingTurns--;
                if (_statuses[i].IsExpired)
                {
                    _statuses.RemoveAt(i);
                }
            }
        }

        // Applies a skip status's own per-turn damage directly, bypassing
        // ModifyIncomingDamage, so a coma's damage is never re-taxed by its own
        // bonusDamageTakenPerHit/incomingDamageMultiplier (prevents a confusing
        // double-dip if a status ever authors both fields).
        public void ApplyStatusTickDamage(int amount)
        {
            ApplyDamageCore(amount);
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

        // Inverse of TrySpendResources — used only when a skill card opts
        // into refundOnAbort and its input minigame was aborted. Not
        // clamped to MaxAp/MaxMp since the amounts refunded were spent from
        // this same participant moments earlier.
        public void RefundResources(int apCost, int mpCost)
        {
            currentAp += apCost;
            currentMp += mpCost;
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
            var liveStates = instantiatedRoot.GetComponentsInChildren<CharacterState>(true);
            foreach (var assetSideRef in definition.EnumerateCharacterStateReferences())
            {
                if (assetSideRef == null || _liveStateByAssetReference.ContainsKey(assetSideRef))
                {
                    continue;
                }

                // A skill card's state is matched by its card; any other by type.
                var live = assetSideRef.SkillCard != null
                    ? Array.Find(liveStates, state => state.SkillCard == assetSideRef.SkillCard)
                    : instantiatedRoot.GetComponentInChildren(assetSideRef.GetType(), true) as CharacterState;
                _liveStateByAssetReference[assetSideRef] = live;
            }
        }

        // Asset-side state that plays card on this character.
        public CharacterState SkillCardStateOf(SkillCardDefinition card)
        {
            return definition.FindSkillCardState(card);
        }

        public CharacterState ResolveLiveState(CharacterState assetSideReference)
        {
            return _liveStateByAssetReference.TryGetValue(assetSideReference, out var live) ? live : null;
        }

        // Notifies every passive on this participant that a skill card has
        // finished resolving (its effects have run) — once per card, from
        // either the one-shot or the live skill-card path.
        public void NotifySkillCardResolved(IBattleQuery battle, SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets, SkillPerformance performance)
        {
            Passives.NotifySkillCardResolved(new PassiveContext(battle, this, card, targets, performance));
        }

        FactionTag ICombatTarget.Faction => faction;
        bool ICombatTarget.IsDefeated => IsDefeated;
        StatBlock ICombatTarget.CurrentStats => GetCurrentStats();
        void ICombatTarget.ApplyDamage(int amount) => ApplyDamage(amount);
        PassiveRuntimeSet ICombatTarget.Passives => Passives;
        PalateProfile ICombatTarget.Palate => Palate;
        int ICombatTarget.Fullness => Fullness;
        FeedResult ICombatTarget.Feed(int amount) => Feed(amount);
        void ICombatTarget.ApplyStatus(StatusEffectInstance status) => ApplyStatus(status);
    }
}
