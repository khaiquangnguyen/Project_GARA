using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Combat
{
    // The player-facing turn loop: one Combat Phase per turn, in which skill
    // cards are repeatable — each gated only by AP+MP affordability and the
    // actor's executor being idle between uses — until the player ends the
    // phase with EndPhase (Space). A/D move the card highlight and
    // UseSkill (Enter) selects the highlighted card; the same public calls (see
    // "Skill card selection" below) are open to any other driver. A
    // card can come from anywhere in the actor's combat loadout
    // (CombatParticipant.SkillCards), whatever its length. Using a card
    // first selects it and opens its targeting (see "Target picking"); it
    // only fires once its targets are confirmed. AI-controlled turns go to
    // PerformAiTurn (see .EnemyTurn) — no input is read for them.
    public partial class CombatSceneManager
    {
        // Raised the instant a stunned (food-coma'd) actor's turn is skipped
        // — carries the actor's portrait so UI can show a "turn skipped"
        // announcement.
        public static event Action<Sprite> TurnSkipped;

        // Raised whenever the highlighted skill card changes, including when
        // a player's phase begins. Carries the card's index in the actor's
        // list — -1 and a null card when the actor has no cards.
        public static event Action<int, SkillCardDefinition> HighlightedSkillCardChanged;

        // Raised whenever a picked-target card's targets change — when
        // picking starts, on every pick/undo, and (with an empty list) when
        // it's cancelled or completes. Carries the picks so far (repeats
        // included, in pick order) and how many are needed in total.
        public static event Action<IReadOnlyList<ICombatTarget>, int> SkillCardTargetsChanged;

        private const int TurnOrderDisplaySlotCount = 6;

        [Header("Combat Phase")]
        [SerializeField] private InputActionAsset combatInputActions;
        [SerializeField] private TargetSelector targetSelector;
        [SerializeField] private SkillCardInputHost skillCardInputHost;
        [SerializeField] private float stunnedTurnDisplaySeconds = 0.6f;

        [Tooltip("Seconds between the battle being set up (characters spawned, turn order shown) and its first turn starting.")]
        [SerializeField] private float battleStartDelaySeconds = 1f;

        private BattleContext _battle;
        private Dictionary<CombatParticipant, AttackExecutor> _executors;
        private ISkillInputSession _activeSession;
        private PhaseActionState _phaseActionState;

        private InputAction _targetLeft;
        private InputAction _targetRight;
        private InputAction _endPhase;
        private InputAction _cancel;
        private InputAction _useSkill;
        private InputAction _skillLeft;
        private InputAction _skillRight;

        private CombatParticipant _actor;
        private AttackExecutor _actorExecutor;
        private bool _phaseActive;
        private int _highlightedCardIndex = -1;

        // Target-picking state — only meaningful while
        // _phaseActionState == SkillCardTargeting.
        private SkillCardDefinition _targetingCard;
        private readonly List<ICombatTarget> _pickedTargets = new();
        private int _requiredTargetCount;

        private void AwakePhase()
        {
            var map = combatInputActions.FindActionMap("Combat");
            _targetLeft = map.FindAction("TargetLeft");
            _targetRight = map.FindAction("TargetRight");
            _endPhase = map.FindAction("EndPhase");
            _cancel = map.FindAction("Cancel");
            _useSkill = map.FindAction("UseSkill");
            _skillLeft = map.FindAction("SkillLeft");
            _skillRight = map.FindAction("SkillRight");
            _parry = map.FindAction("Parry");
            _jump = map.FindAction("Jump");
        }

        private void EnablePhase()
        {
            _targetLeft.performed += OnTargetLeft;
            _targetRight.performed += OnTargetRight;
            _endPhase.performed += OnEndCombatPhase;
            _cancel.performed += OnCancel;
            _useSkill.performed += OnUseSkill;
            _skillLeft.performed += OnSkillLeft;
            _skillRight.performed += OnSkillRight;

            EnableParry();
            EnableJump();
            CombatParticipant.Defeated += OnParticipantDefeated;
            CombatParticipant.StatusApplied += OnParticipantStatusApplied;

            combatInputActions.FindActionMap("Combat").Enable();
        }

        private void DisablePhase()
        {
            _targetLeft.performed -= OnTargetLeft;
            _targetRight.performed -= OnTargetRight;
            _endPhase.performed -= OnEndCombatPhase;
            _cancel.performed -= OnCancel;
            _useSkill.performed -= OnUseSkill;
            _skillLeft.performed -= OnSkillLeft;
            _skillRight.performed -= OnSkillRight;

            DisableParry();
            DisableJump();
            CombatParticipant.Defeated -= OnParticipantDefeated;
            CombatParticipant.StatusApplied -= OnParticipantStatusApplied;

            combatInputActions.FindActionMap("Combat").Disable();

            AbortActiveSkillCardSession();
            UnbindNoirWorld();
        }

        // Called by CombatManager once every participant is spawned and bound.
        public void BeginBattle(BattleContext battle, Dictionary<CombatParticipant, AttackExecutor> executors)
        {
            _battle = battle;
            _executors = executors;
            BindNoirWorld(_battle.NoirWorld);

            _battle.InitializeTurnOrder();
            RaiseTurnOrderChanged();
            StartCoroutine(BeginFirstPhaseAfterDelay());
        }

        private IEnumerator BeginFirstPhaseAfterDelay()
        {
            yield return new WaitForSeconds(battleStartDelaySeconds);
            BeginPhaseForCurrentActor();
        }

        // Safety rail: cuts short whatever skill-card input session is
        // running (if any) and resets the flow back to Regular. Called from
        // EndCombatPhase and OnDisable so a phase never ends, and this
        // component never gets disabled, mid-minigame.
        private void AbortActiveSkillCardSession()
        {
            if (_activeSession != null)
            {
                _activeSession.Abort();
                _activeSession = null;
            }

            ClearTargetPicking();
            HideTargetDisplays();
            SnapRetreatedBack();
            _phaseActionState = PhaseActionState.Regular;
        }

        // Drops the just-defeated participant from whatever's currently
        // selectable (a no-op if it wasn't in the pool) and refreshes the
        // turn-order display — fires the instant a participant dies,
        // regardless of whose turn it is or what killed them.
        private void OnParticipantDefeated(CombatParticipant participant)
        {
            targetSelector.RemoveCandidate(participant);
            if (IsPickingSkillCardTargets)
            {
                RefreshTargetPreview();
            }

            NotifyPassivesOfDefeat(participant);

            RaiseTurnOrderChanged();
        }

        // Slot 0 is always whoever's turn it currently is — the queue
        // itself shifts left as each turn is consumed, rather than a fixed
        // per-round slot being highlighted in place.
        // A status (e.g. food coma) may have changed Speed.
        private void OnParticipantStatusApplied(CombatParticipant participant)
        {
            _battle.RecalculateTurnOrder();
            RaiseTurnOrderChanged();
        }

        private void RaiseTurnOrderChanged()
        {
            var upcoming = _battle.GetUpcomingQueue(TurnOrderDisplaySlotCount);
            var portraits = new Sprite[upcoming.Count];
            for (var i = 0; i < upcoming.Count; i++)
            {
                portraits[i] = upcoming[i].definition.portrait;
            }

            RefreshTurnOrder(portraits);
        }

        // ---- Skill card selection ----
        // Input-agnostic: nothing here reads input. Whatever drives card
        // selection (shortcut keys, a card-hand UI, ...) calls these. Every
        // call is a no-op returning false outside a player's actionable
        // moment (see CanSelectSkillCards).

        // True while the current player actor can highlight/use cards —
        // their Combat Phase is active and no card is mid-minigame/swing.
        public bool CanSelectSkillCards => CanAct();

        // The current actor's combat loadout — empty when nobody's acting.
        public IReadOnlyList<SkillCardDefinition> CurrentSkillCards =>
            _actor != null ? _actor.SkillCards : Array.Empty<SkillCardDefinition>();

        // Index into CurrentSkillCards, or -1 when there's nothing to highlight.
        public int HighlightedSkillCardIndex => _highlightedCardIndex;

        public bool HighlightSkillCard(int index)
        {
            return CanAct() && SetCardHighlight(index);
        }

        // Moves the highlight by step (e.g. -1/+1), wrapping around at
        // either end of the loadout.
        public bool MoveSkillCardHighlight(int step)
        {
            if (!CanAct())
            {
                return false;
            }

            var count = _actor.SkillCards.Count;
            if (count == 0)
            {
                return false;
            }

            return SetCardHighlight(((_highlightedCardIndex + step) % count + count) % count);
        }

        // Highlights the card too, so a direct use and a highlight-then-use
        // leave the same highlight behind.
        public bool UseSkillCard(int index)
        {
            return CanAct() && SetCardHighlight(index) && TryUseSkillCard(index);
        }

        public bool UseHighlightedSkillCard()
        {
            return TryUseSkillCard(_highlightedCardIndex);
        }

        // ---- Target picking ----
        // Using a card enters SkillCardTargeting over the card's pool: One/
        // Multi pick RequiredSkillCardTargetCount times off the selector's
        // cursor, All selects the whole pool and needs one confirm. The card
        // fires on the last pick, and resources are only spent then, so
        // cancelling costs nothing. Target displays (HP hearts, ...) show
        // only for whoever's currently selected.

        public bool IsPickingSkillCardTargets => _phaseActionState == PhaseActionState.SkillCardTargeting;

        public IReadOnlyList<ICombatTarget> PickedSkillCardTargets => _pickedTargets;

        public int RequiredSkillCardTargetCount => IsPickingSkillCardTargets ? _requiredTargetCount : 0;

        // Adds the selector's current target. Fails without a repeat mode if
        // that character has already been picked.
        public bool PickSkillCardTarget()
        {
            if (!IsPickingSkillCardTargets || !targetSelector.HasCandidates)
            {
                return false;
            }

            if (_targetingCard.targetMode.IsAll())
            {
                _pickedTargets.AddRange(targetSelector.Candidates);
            }
            else
            {
                var target = targetSelector.CurrentTarget;
                if (!_targetingCard.targetMode.AllowsRepeatTargets() && _pickedTargets.Contains(target))
                {
                    return false;
                }

                _pickedTargets.Add(target);
            }

            if (!_targetingCard.targetMode.IsAll() && _pickedTargets.Count < _requiredTargetCount)
            {
                OnPickedTargetsChanged();
                return true;
            }

            var card = _targetingCard;
            var targets = _pickedTargets.ToArray();
            ClearTargetPicking();
            _phaseActionState = PhaseActionState.Regular;
            return CommitSkillCard(card, targets);
        }

        public bool UndoSkillCardTarget()
        {
            if (!IsPickingSkillCardTargets || _pickedTargets.Count == 0)
            {
                return false;
            }

            _pickedTargets.RemoveAt(_pickedTargets.Count - 1);
            OnPickedTargetsChanged();
            return true;
        }

        // Drops every pick and returns to Regular without using the card.
        public bool CancelSkillCardTargeting()
        {
            if (!IsPickingSkillCardTargets)
            {
                return false;
            }

            ClearTargetPicking();
            _phaseActionState = PhaseActionState.Regular;
            return true;
        }

        private void BeginTargetPicking(SkillCardDefinition card, List<CombatParticipant> pool, int requiredTargetCount)
        {
            _targetingCard = card;
            _requiredTargetCount = requiredTargetCount;
            _pickedTargets.Clear();
            _phaseActionState = PhaseActionState.SkillCardTargeting;
            targetSelector.BeginSelection(pool, card.targetMode.IsAll());
            RefreshTargetPreview();
            SkillCardTargetsChanged?.Invoke(_pickedTargets, _requiredTargetCount);
        }

        private void OnPickedTargetsChanged()
        {
            targetSelector.SetMarked(_pickedTargets.OfType<CombatParticipant>());
            RefreshTargetPreview();
            SkillCardTargetsChanged?.Invoke(_pickedTargets, _requiredTargetCount);
        }

        private void RefreshTargetPreview()
        {
            ShowTargetDisplays(targetSelector.Selected);
        }

        // Discards every targeting visual — the indicators and the preview's
        // target displays. Runs before a card fires, so only its announced
        // targets' displays come back (see AnnounceTargetingForSkillCard).
        private void ClearTargetPicking()
        {
            if (_targetingCard == null)
            {
                return;
            }

            targetSelector.EndSelection();
            HideTargetDisplays();
            _targetingCard = null;
            _requiredTargetCount = 0;
            _pickedTargets.Clear();
            SkillCardTargetsChanged?.Invoke(_pickedTargets, 0);
        }

        private bool SetCardHighlight(int index)
        {
            if (!_actor.TryGetSkillCard(index, out var card))
            {
                return false;
            }

            _highlightedCardIndex = index;
            HighlightSkillCardSlot(index);
            HighlightedSkillCardChanged?.Invoke(index, card);
            return true;
        }

        private void ResetCardHighlight()
        {
            _highlightedCardIndex = -1;
            if (_actor.SkillCards.Count > 0)
            {
                SetCardHighlight(0);
            }
            else
            {
                HighlightSkillCardSlot(-1);
                HighlightedSkillCardChanged?.Invoke(-1, null);
            }
        }

        // A/D move the card highlight left/right, wrapping around.
        private void OnSkillLeft(InputAction.CallbackContext ctx)
        {
            MoveSkillCardHighlight(-1);
        }

        private void OnSkillRight(InputAction.CallbackContext ctx)
        {
            MoveSkillCardHighlight(1);
        }

        // Enter selects the highlighted card, or confirms a target while picking.
        private void OnUseSkill(InputAction.CallbackContext ctx)
        {
            if (IsPickingSkillCardTargets)
            {
                PickSkillCardTarget();
                return;
            }

            UseHighlightedSkillCard();
        }

        private void OnTargetLeft(InputAction.CallbackContext ctx)
        {
            if (IsPickingSkillCardTargets)
            {
                targetSelector.CycleLeft();
                RefreshTargetPreview();
            }
        }

        private void OnTargetRight(InputAction.CallbackContext ctx)
        {
            if (IsPickingSkillCardTargets)
            {
                targetSelector.CycleRight();
                RefreshTargetPreview();
            }
        }

        // Esc steps back while picking — undoes the last pick, then
        // drops the card.
        private void OnCancel(InputAction.CallbackContext ctx)
        {
            if (IsPickingSkillCardTargets && !UndoSkillCardTarget())
            {
                CancelSkillCardTargeting();
            }
        }

        // Space ends the phase.
        private void OnEndCombatPhase(InputAction.CallbackContext ctx)
        {
            if (!CanAct())
            {
                return;
            }

            EndCombatPhase();
        }

        private bool CanAct()
        {
            return _phaseActive
                   && _actor != null
                   && _actor.IsPlayerControlled
                   && !_actorExecutor.IsBusy
                   && _phaseActionState == PhaseActionState.Regular;
        }

        // Selects the card and opens its targeting (see "Target picking") —
        // nothing fires or is spent yet. Fails if the card's unaffordable or
        // its pool is empty. While a card is targeting, mid-minigame or
        // resolving, card selection and EndPhase are ignored (see CanAct).
        private bool TryUseSkillCard(int index)
        {
            if (!CanAct() || !_actor.TryGetSkillCard(index, out var card))
            {
                return false;
            }

            if (_actor.currentAp < card.apCost || _actor.currentMp < card.mpCost)
            {
                return false;
            }

            var pool = LivingPoolOf(_actor, card.targetMode.GetPool());
            var requiredTargetCount = card.targetMode.IsAll() ? pool.Count
                : card.targetMode.IsMulti() ? card.targetMode.ResolveMultiTargetCount(card.multiTargetCount, pool.Count)
                : Mathf.Min(1, pool.Count);
            if (requiredTargetCount == 0)
            {
                return false;
            }

            BeginTargetPicking(card, pool, requiredTargetCount);
            return true;
        }

        // Spends the card's cost and starts its input minigame against the
        // already-resolved targets.
        private bool CommitSkillCard(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets)
        {
            if (skillCardInputHost == null)
            {
                Debug.LogError($"[{nameof(CombatSceneManager)}] No {nameof(SkillCardInputHost)} assigned — can't use {card.name}.", this);
                return false;
            }

            if (!_actor.TrySpendResources(card.apCost, card.mpCost))
            {
                return false;
            }

            StartSkillCard(card, targets);
            return true;
        }

        // Plays the card for the current actor, whoever drives it (the
        // player once it's paid for, or PerformAiTurn). Resolving ends
        // once _phaseActionState is back to Regular.
        private void StartSkillCard(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets)
        {
            AnnounceActiveActor(_actor, false);
            AnnounceTargetingForSkillCard(_actor, targets);
            RetreatUninvolved(_actor, targets);

            _phaseActionState = PhaseActionState.SkillCardInput;
            skillCardInputHost.RaiseInputPhaseStarted(card);

            skillCardInputHost.Actor = _actor.definition;
            _activeSession = card.CreateInputSession(skillCardInputHost);
            if (_activeSession is ILiveSkillInputSession liveSession)
            {
                BeginLiveSkillCard(card, targets, liveSession);
                return;
            }

            _activeSession.Begin(performance => OnSkillCardInputCompleted(card, targets, performance));
        }

        // Fires once the card's input minigame finishes (including an
        // abort). WasAborted + refundOnAbort skips the animation/effects
        // entirely and refunds the spend; otherwise the swing plays and
        // every authored effect resolves at the animation's impact frame
        // (see AttackExecutor.PlayAction's onImpact).
        private void OnSkillCardInputCompleted(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets, SkillPerformance performance)
        {
            _activeSession = null;
            _phaseActionState = PhaseActionState.SkillCardResolving;
            skillCardInputHost.RaiseInputPhaseEnded(performance);

            if (performance.WasAborted && card.refundOnAbort)
            {
                _actor.RefundResources(card.apCost, card.mpCost);
                AnnounceTargetingClearedForSkillCard(_actor);
                StartCoroutine(FinishSkillCardResolving(_actorExecutor));
                return;
            }

            var battleQuery = _battle.QueryFor(_actor);
            var actingParticipant = _actor;
            _actorExecutor.PlayAction(_actor.SkillCardStateOf(card), targets, card.positionMode, onImpact: () =>
            {
                foreach (var effect in card.effects)
                {
                    if (effect != null)
                    {
                        effect.Resolve(new SkillEffectContext(battleQuery, actingParticipant, targets, performance));
                    }
                }

                actingParticipant.NotifySkillCardResolved(battleQuery, card, targets, performance);
            });
            _actorExecutor.ActionFinished += OnSkillCardActionFinished;

            void OnSkillCardActionFinished()
            {
                _actorExecutor.ActionFinished -= OnSkillCardActionFinished;
                StartCoroutine(EndSkillCard(_actor, _actorExecutor, card.endDelay));
            }
        }

        // Holds the actor's last pose for holdSeconds, then walks everyone
        // back and finishes resolving.
        private IEnumerator EndSkillCard(CombatParticipant actor, AttackExecutor actorExecutor, float holdSeconds)
        {
            if (holdSeconds > 0f)
            {
                yield return new WaitForSeconds(holdSeconds);
            }

            AnnounceTargetingClearedForSkillCard(actor);
            actorExecutor.ReturnToStandardPosition();
            yield return FinishSkillCardResolving(actorExecutor);
        }

        // Resolving only ends — and input only unlocks — once the actor and
        // everyone who retreated for this card are back in formation, so the
        // next card never starts while someone's still walking back.
        private IEnumerator FinishSkillCardResolving(AttackExecutor actorExecutor)
        {
            yield return ReturnRetreated();
            yield return new WaitUntil(() => !actorExecutor.IsBusy);
            _phaseActionState = PhaseActionState.Regular;
        }

        private void BeginPhaseForCurrentActor()
        {
            _actor = _battle.CurrentActor;
            if (_actor == null)
            {
                Debug.LogError($"[{nameof(CombatSceneManager)}] No one to act — is the roster (RogueRunManager) empty?", this);
                return;
            }

            _actorExecutor = _executors[_actor];
            SetSortingOrder(_actor, 1);
            AnnounceActiveActor(_actor, true);
            ToggleTurnAnnouncement(_actor.IsPlayerControlled);
            RefreshSkillCardSlots(_actor);

            // Timed modifiers (see TimedStatModifierSkillEffect) tick down
            // at the start of THEIR OWNER's own turn, not on every global
            // turn. If that changed the actor's Speed, the not-yet-acted
            // remainder of the turn order needs to be re-sorted.
            var speedBeforeTick = _actor.GetCurrentStats().Speed.Value;
            _actor.TickTimedModifiers();
            if (_actor.GetCurrentStats().Speed.Value != speedBeforeTick)
            {
                _battle.RecalculateTurnOrder();
            }

            // Cooking (Chef): a food-coma'd actor's statuses tick here, at
            // the start of their own phase, same as timed modifiers above.
            // If they're still stunned they never read input or take an
            // enemy turn — their whole phase is just the coma's own damage
            // tick and a brief display beat.
            _actor.TickStatuses();

            if (_actor.IsStunned)
            {
                _phaseActive = false;
                StartCoroutine(SkipStunnedTurn());
                return;
            }

            if (!_actor.IsPlayerControlled)
            {
                _phaseActive = false;
                StartCoroutine(PerformAiTurn(_actor, _actorExecutor));
                return;
            }

            _phaseActive = true;
            ResetCardHighlight();
        }

        // A stunned actor takes its food-coma damage, burns one turn of the
        // status, and ends its phase without ever reading input or taking
        // an enemy turn.
        private IEnumerator SkipStunnedTurn()
        {
            var participant = _actor;
            TurnSkipped?.Invoke(participant.definition.portrait);
            var comaDamage = participant.PendingSkippedTurnDamage;
            if (comaDamage > 0)
            {
                participant.ApplyStatusTickDamage(comaDamage);
            }

            participant.ConsumeSkippedTurn();
            yield return new WaitForSeconds(stunnedTurnDisplaySeconds);
            EndCombatPhase();
        }

        private void EndCombatPhase()
        {
            AbortActiveSkillCardSession();

            _phaseActive = false;
            _aiActionInProgress = false;
            targetSelector.EndSelection();
            AnnounceTargetingClearedForEnemies(_actor);
            AnnounceActiveActor(_actor, false);
            SetSortingOrder(_actor, 0);

            // An extra turn (e.g. Dancer's Encore) holds the cursor so the
            // same actor starts a fresh phase.
            var extraTurn = !_actor.IsDefeated && _actor.Passives.TryConsumeExtraTurn();
            if (!extraTurn)
            {
                _battle.AdvanceTurn();
            }

            RaiseTurnOrderChanged();

            if (_battle.IsBattleOver)
            {
                _battle.NoirWorld.Exit();
                return;
            }

            _battle.NoirWorld.TickTurn();
            BeginPhaseForCurrentActor();
        }

        private void AnnounceTargetingClearedForEnemies(CombatParticipant actor)
        {
            foreach (var enemy in LivingEnemiesOf(actor))
            {
                MMEventManager.TriggerEvent(new TargetedStateEvent(enemy.SceneRoot, true));
            }
        }

        // Announces the card's targets as targeted and everyone not involved
        // (neither actor nor target, on either side) as not-targeted.
        // Listeners (see OnNotTargetedEffect) decide what that looks like.
        // Targets keep their target displays until the card is cleared.
        private void AnnounceTargetingForSkillCard(CombatParticipant actor, IReadOnlyList<ICombatTarget> targets)
        {
            ShowTargetDisplays(targets.OfType<CombatParticipant>());

            foreach (var participant in LivingUninvolvedIn(actor, targets))
            {
                MMEventManager.TriggerEvent(new TargetedStateEvent(participant.SceneRoot, false));
            }

            foreach (var target in targets.OfType<CombatParticipant>())
            {
                MMEventManager.TriggerEvent(new TargetedStateEvent(target.SceneRoot, true));
            }
        }

        private void AnnounceTargetingClearedForSkillCard(CombatParticipant actor)
        {
            HideTargetDisplays();
            foreach (var participant in LivingEnemiesOf(actor).Concat(LivingAlliesOf(actor)))
            {
                MMEventManager.TriggerEvent(new TargetedStateEvent(participant.SceneRoot, true));
            }
        }

        // Renders the acting character above the rest of their formation
        // for the duration of their turn — order 1 while it's their turn,
        // reset to 0 once it ends — and a retreated character at its retreat
        // slot's own order (see .Retreat). Sorting order itself lives on the
        // MeshRenderer Spine renders through, but resolved off the
        // SkeletonRenderer specifically (its own GameObject's renderer) so
        // this targets the actual skeleton's renderer, not just whichever
        // MeshRenderer happens to be found first under the character.
        private void SetSortingOrder(CombatParticipant participant, int order)
        {
            if (participant?.SceneRoot == null)
            {
                return;
            }

            var skeletonRenderer = participant.SceneRoot.GetComponentInChildren<SkeletonRenderer>();
            if (skeletonRenderer == null)
            {
                return;
            }

            var meshRenderer = skeletonRenderer.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = order;
            }
        }

        private List<CombatParticipant> LivingEnemiesOf(CombatParticipant actor)
        {
            return LivingWhere(p => p.Allegiance != actor.Allegiance);
        }

        private List<CombatParticipant> LivingAlliesOf(CombatParticipant actor)
        {
            return LivingWhere(p => p.Allegiance == actor.Allegiance);
        }

        // Every living character on either side that's neither the actor
        // nor one of the action's targets.
        private List<CombatParticipant> LivingUninvolvedIn(CombatParticipant actor, IReadOnlyList<ICombatTarget> targets)
        {
            return LivingEnemiesOf(actor).Concat(LivingAlliesOf(actor))
                .Where(participant => participant != actor && !targets.Contains(participant))
                .ToList();
        }

        // Slot order: the players' side first, then the enemies'.
        private List<CombatParticipant> LivingWhere(Func<CombatParticipant, bool> predicate)
        {
            return _battle.playerParty.LivingMembers().Concat(_battle.enemyParty.LivingMembers()).Where(predicate).ToList();
        }

        private List<CombatParticipant> LivingPoolOf(CombatParticipant actor, TargetPool pool)
        {
            switch (pool)
            {
                case TargetPool.Friendlies:
                    return LivingAlliesOf(actor);
                case TargetPool.Everyone:
                    return LivingEnemiesOf(actor).Concat(LivingAlliesOf(actor)).ToList();
                default:
                    return LivingEnemiesOf(actor);
            }
        }
    }
}
