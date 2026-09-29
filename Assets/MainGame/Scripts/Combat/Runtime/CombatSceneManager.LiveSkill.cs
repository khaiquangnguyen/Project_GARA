using System.Collections;
using System.Collections.Generic;
using GARA.Characters;
using Spine.Unity;
using UnityEngine;

namespace GARA.Combat
{
    // Live skill cards (ILiveSkillInputSession): the actor dashes in to the
    // opening move's range before the minigame, then plays each step's
    // attack on the card's own state as the session emits it, resolving the
    // step's effects on its hit. Bar steps play at the card's positionMode;
    // the finale plays animationSpec at finalePositionMode and drops the spec's announcement onto each target so
    // it lands on the finale's hit frame. Each hit plays its spec's
    // feedback prefab on the actor.
    public partial class CombatSceneManager
    {
        private void BeginLiveSkillCard(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets, ILiveSkillInputSession session)
        {
            var battleQuery = _battle.QueryFor(_actor);
            var actor = _actor;
            var executor = _performerExecutor;
            var state = PerformerStateOf(card);
            var stepsLanded = 0;
            var finalePending = false;
            var stepPicks = new Dictionary<int, IReadOnlyList<ICombatTarget>>();
            var struck = new List<ICombatTarget>();
            ICombatTarget pendingMoveTarget = null;
            AttackAnimationSpec pendingMoveSpec = null;
            var pendingMoveIndex = -1;
            var sequential = session is ISequentialLiveSkillInputSession;
            var queuedSteps = new Queue<SkillStep>();
            var spotlight = ShowSpotlight(card, targets);
            var shadowScreen = ShowShadowScreen(card, _performer ?? actor);

            session.StepPerformed += OnStep;
            session.StepStarting += OnStepStarting;
            executor.ActionFinished += OnStepFinished;

            if (shadowScreen != null && shadowScreen.IntroDuration > 0f)
            {
                StartCoroutine(StartAfterIntro(shadowScreen.IntroDuration));
            }
            else
            {
                BeginCard();
            }

            // Lets the shadow screen close and light up before the first move.
            IEnumerator StartAfterIntro(float seconds)
            {
                yield return new WaitForSeconds(seconds);
                BeginCard();
            }

            void BeginCard()
            {
                if (card.positionMode == ActionPositionMode.MoveInFrontOfEnemy && targets.Count > 0)
                {
                    executor.MoveInFrontOf(TargetsOf(false, 0)[0], state, BeginSession, session.OpeningMove);
                }
                else
                {
                    BeginSession();
                }
            }

            void BeginSession()
            {
                // The phase may have ended during the dash.
                if (_activeSession != session)
                {
                    Unsubscribe();
                    return;
                }

                session.Begin(OnCompleted);
            }

            void OnStep(SkillStep step)
            {
                if (step.isFinale)
                {
                    finalePending = true;
                    StartCoroutine(PlayFinaleAfterDelay(step));
                    return;
                }

                if (sequential && (executor.IsBusy || queuedSteps.Count > 0))
                {
                    queuedSteps.Enqueue(step);
                    return;
                }

                PlayStep(step);
            }

            // Lets the last step's swing finish, then holds finaleDelay.
            IEnumerator PlayFinaleAfterDelay(SkillStep step)
            {
                yield return new WaitUntil(() => !executor.IsBusy && queuedSteps.Count == 0);
                if (card.finaleDelay > 0f)
                {
                    yield return new WaitForSeconds(card.finaleDelay);
                }

                PlayStep(step);
                finalePending = false;
            }

            void PlayStep(SkillStep step)
            {
                stepsLanded++;
                List<PerfectAnnouncementDropEffect> drops = null;
                var positionMode = step.isFinale ? card.finalePositionMode : card.positionMode;
                var stepTargets = TargetsOf(step.isFinale, step.index);
                if (pendingMoveIndex == step.index)
                {
                    pendingMoveTarget = null;
                }

                if (step.isFinale && card.alliesJoinFinale)
                {
                    BringAlliesBackToDance(actor, card.allyDanceDuration);
                }

                var teleport = card.teleportBetweenSteps && !step.isFinale;
                executor.PlayAction(state, stepTargets, positionMode, spec: step.animation, beforeAttack: BeforeAttack, teleport: teleport, onImpact: () =>
                {
                    if (step.whiffs)
                    {
                        return;
                    }

                    var move = step.animation != null ? step.animation : card.animationSpec;
                    executor.PlayHitFeedback(move != null ? move.HitFeedback : null);
                    if (step.isFinale && move is FinaleAttackAnimationSpec finale)
                    {
                        executor.PlayHitFeedback(finale.LightEffect);
                    }

                    ApplyStepEffects();
                    LandDrops();
                    if (step.isFinale && spotlight != null)
                    {
                        spotlight.Hit();
                    }

                    if (step.isFinale && shadowScreen != null)
                    {
                        shadowScreen.Hit();
                    }
                });

                void ApplyStepEffects()
                {
                    if (step.effects == null)
                    {
                        return;
                    }

                    if (!step.isFinale)
                    {
                        foreach (var target in stepTargets)
                        {
                            if (!struck.Contains(target))
                            {
                                struck.Add(target);
                            }
                        }
                    }

                    CombatParticipant.DamageSource = actor;
                    try
                    {
                        foreach (var effect in step.effects)
                        {
                            if (effect != null)
                            {
                                effect.ApplyEffect(new SkillEffectContext(battleQuery, actor, stepTargets, step.performance));
                            }
                        }
                    }
                    finally
                    {
                        CombatParticipant.DamageSource = null;
                    }
                }

                void LandDrops()
                {
                    if (drops == null)
                    {
                        return;
                    }

                    foreach (var drop in drops)
                    {
                        if (drop != null)
                        {
                            drop.Land();
                        }
                    }
                }

                // Runs right before the swing, so the drop is timed from it.
                float BeforeAttack()
                {
                    if (!step.isFinale)
                    {
                        return 0f;
                    }

                    drops = DropPerfectAnnouncements(card, state, stepTargets, executor, out var leadIn);
                    return leadIn;
                }
            }

            // Moves in front of the starting step's own target, so the
            // performer is already there while its input plays.
            void OnStepStarting(int index, AttackAnimationSpec move)
            {
                if (card.positionMode != ActionPositionMode.MoveInFrontOfEnemy || move == null || targets.Count == 0)
                {
                    return;
                }

                pendingMoveTarget = TargetsOf(false, index)[0];
                pendingMoveSpec = move;
                pendingMoveIndex = index;
                if (!executor.IsBusy)
                {
                    MoveToPendingTarget();
                }
            }

            void MoveToPendingTarget()
            {
                if (pendingMoveTarget == null || _activeSession != session)
                {
                    return;
                }

                var target = pendingMoveTarget;
                pendingMoveTarget = null;
                executor.MoveInFrontOf(target, state, null, pendingMoveSpec, card.teleportBetweenSteps);
            }

            // A step's picks are decided once, so getting into position and
            // the swing agree; a random pick that died since is re-picked.
            IReadOnlyList<ICombatTarget> TargetsOf(bool isFinale, int index)
            {
                if (isFinale && card.finaleTargeting == FinaleTargeting.PicksStruckBySteps)
                {
                    return struck;
                }

                if (isFinale || index < 0)
                {
                    return StepTargets(card, targets, isFinale, index);
                }

                if (!stepPicks.TryGetValue(index, out var picks)
                    || (card.stepTargeting == StepTargeting.RandomLivingPick && picks.Count > 0 && picks[0].IsDefeated))
                {
                    picks = StepTargets(card, targets, false, index);
                    stepPicks[index] = picks;
                }

                return picks;
            }

            void OnStepFinished()
            {
                executor.ReturnToIdle();
                if (queuedSteps.Count > 0)
                {
                    PlayStep(queuedSteps.Dequeue());
                    return;
                }

                MoveToPendingTarget();
            }

            void OnCompleted(SkillPerformance performance)
            {
                session.StepPerformed -= OnStep;
                session.StepStarting -= OnStepStarting;
                pendingMoveTarget = null;
                _activeSession = null;
                _phaseActionState = PhaseActionState.SkillCardResolving;
                if (performance.WasAborted)
                {
                    queuedSteps.Clear();
                }

                skillCardInputHost.RaiseInputPhaseEnded(performance);

                var refunded = performance.WasAborted && card.refundOnAbort && stepsLanded == 0;
                if (refunded)
                {
                    actor.RefundResources(card.apCost, card.mpCost);
                }

                StartCoroutine(FinishLiveSkillCard(card, targets, performance, !refunded));
            }

            IEnumerator FinishLiveSkillCard(SkillCardDefinition finishedCard, IReadOnlyList<ICombatTarget> finishedTargets, SkillPerformance performance, bool notify)
            {
                yield return new WaitUntil(() => !executor.IsBusy && !finalePending && queuedSteps.Count == 0);
                Unsubscribe();
                if (spotlight != null)
                {
                    spotlight.Hit();
                }

                if (shadowScreen != null)
                {
                    shadowScreen.Hit();
                }

                if (notify)
                {
                    actor.NotifySkillCardResolved(battleQuery, finishedCard, finishedTargets, performance);
                }

                yield return EndSkillCard(actor, executor, notify ? finishedCard.endDelay : 0f);
            }

            void Unsubscribe()
            {
                session.StepPerformed -= OnStep;
                session.StepStarting -= OnStepStarting;
                executor.ActionFinished -= OnStepFinished;
            }
        }
    
        // Every pick, unless the card narrows it: a step per stepTargeting,
        // the finale per finaleTargeting (struck picks are tracked by the
        // caller).
        private static IReadOnlyList<ICombatTarget> StepTargets(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets, bool isFinale, int index)
        {
            if (targets.Count == 0)
            {
                return targets;
            }

            if (isFinale)
            {
                return card.finaleTargeting == FinaleTargeting.OneRandomPick ? new[] { RandomLivingPick(targets) } : targets;
            }

            if (index < 0)
            {
                return targets;
            }

            switch (card.stepTargeting)
            {
                case StepTargeting.TakeTurns:
                    return new[] { targets[index % targets.Count] };
                case StepTargeting.RandomLivingPick:
                    return new[] { RandomLivingPick(targets) };
                default:
                    return targets;
            }
        }

        // Any pick when none is left alive.
        private static ICombatTarget RandomLivingPick(IReadOnlyList<ICombatTarget> targets)
        {
            var living = new List<ICombatTarget>();
            foreach (var target in targets)
            {
                if (!target.IsDefeated)
                {
                    living.Add(target);
                }
            }

            var pool = living.Count > 0 ? (IReadOnlyList<ICombatTarget>)living : targets;
            return pool[Random.Range(0, pool.Count)];
        }

        // Spawns the finale spec's drop effect at each target, released so it
        // lands on the finale's hit event. leadIn is how long the finale must
        // wait to start when the drop is longer than its time-to-hit. Null
        // when it isn't a FinaleAttackAnimationSpec with one.
        private static List<PerfectAnnouncementDropEffect> DropPerfectAnnouncements(SkillCardDefinition card, CharacterState state, IReadOnlyList<ICombatTarget> targets, AttackExecutor executor, out float leadIn)
        {
            leadIn = 0f;
            // Unity's == null, not a pattern: an unassigned field is a fake
            // null that a pattern match lets through.
            var template = card.animationSpec is FinaleAttackAnimationSpec finale ? finale.PerfectAnnouncementDrop : null;
            if (template == null)
            {
                return null;
            }

            if (!template.TryGetComponent<PerfectAnnouncementDropEffect>(out var templateEffect))
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] {card.name}: perfect announcement drop has no {nameof(PerfectAnnouncementDropEffect)}.", template);
                return null;
            }

            if (!executor.TryGetSecondsToImpact(state, out var secondsToImpact))
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] {card.name}: finale clip has no hit event — announcement lands at its start.", card);
            }

            leadIn = Mathf.Max(0f, templateEffect.Duration - secondsToImpact);
            var releaseDelay = Mathf.Max(0f, secondsToImpact - templateEffect.Duration);
            var drops = new List<PerfectAnnouncementDropEffect>();
            foreach (var target in targets)
            {
                if (target is CombatParticipant participant && participant.SceneTransform != null)
                {
                    var drop = Instantiate(template).GetComponent<PerfectAnnouncementDropEffect>();
                    drop.Drop(CentreOf(participant), releaseDelay);
                    drops.Add(drop);
                }
            }

            return drops;
        }

        // Spawns the card's spotlight over the targets, held until the caller
        // reports the hit. Null when the card has none.
        private static EncoreSpotlightEffect ShowSpotlight(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets)
        {
            // Unity's == null, as in DropPerfectAnnouncements.
            var template = card.spotlight;
            if (template == null)
            {
                return null;
            }

            if (!template.TryGetComponent<EncoreSpotlightEffect>(out _))
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] {card.name}: spotlight has no {nameof(EncoreSpotlightEffect)}.", template);
                return null;
            }

            var anchors = new List<Transform>();
            var aimOffsets = new List<Vector3>();
            foreach (var target in targets)
            {
                if (target is CombatParticipant participant && participant.SceneTransform != null)
                {
                    anchors.Add(participant.SceneTransform);
                    aimOffsets.Add(CentreOf(participant) - participant.SceneTransform.position);
                }
            }

            var spotlight = Instantiate(template).GetComponent<EncoreSpotlightEffect>();
            spotlight.Show(anchors, aimOffsets);
            return spotlight;
        }

        // Spawns the card's shadow screen around the performer, held until
        // the caller reports the hit. Null when the card has none.
        private static ShadowScreenEffect ShowShadowScreen(SkillCardDefinition card, CombatParticipant performer)
        {
            // Unity's == null, as in DropPerfectAnnouncements.
            var template = card.shadowScreen;
            if (template == null || performer.SceneRoot == null)
            {
                return null;
            }

            if (!template.TryGetComponent<ShadowScreenEffect>(out _))
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] {card.name}: shadow screen has no {nameof(ShadowScreenEffect)}.", template);
                return null;
            }

            var shadowScreen = Instantiate(template).GetComponent<ShadowScreenEffect>();
            shadowScreen.Show(performer.SceneRoot, BodyBoundsOf(performer));
            return shadowScreen;
        }

        // Height assumed for a participant without a Spine mesh.
        private const float FallbackBodyHeight = 2f;

        // Centre of the participant's Spine mesh, or its root if it has none.
        private static Vector3 CentreOf(CombatParticipant participant)
        {
            var bounds = BodyBoundsOf(participant);
            return new Vector3(bounds.center.x, bounds.center.y, participant.SceneTransform.position.z);
        }

        // Bounds of the participant's Spine mesh, or a body standing on its
        // root if it has none.
        private static Bounds BodyBoundsOf(CombatParticipant participant)
        {
            var skeleton = participant.SceneRoot.GetComponentInChildren<SkeletonRenderer>();
            if (skeleton != null && skeleton.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                return meshRenderer.bounds;
            }

            var root = participant.SceneTransform.position;
            return new Bounds(root + Vector3.up * (FallbackBodyHeight * 0.5f), new Vector3(1f, FallbackBodyHeight, 0f));
        }
    }
}
