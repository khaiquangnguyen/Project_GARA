using System.Collections;
using System.Collections.Generic;
using GARA.Characters;
using Spine.Unity;
using UnityEngine;

namespace GARA.Combat
{
    // Live skill cards (ILiveSkillInputSession): the actor dashes in before
    // the minigame, then plays each step's attack on the card's own state and
    // resolves it as the session emits it. Normal steps resolve the card's
    // effects at their share; the finale resolves its perfectEffects and, for
    // an IPerfectAnnouncementCard, drops its announcement onto each target so
    // it lands on the finale's hit frame.
    public partial class CombatSceneManager
    {
        private void BeginLiveSkillCard(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets, ILiveSkillInputSession session)
        {
            var battleQuery = _battle.QueryFor(_actor);
            var actor = _actor;
            var executor = _actorExecutor;
            var state = actor.SkillCardStateOf(card);
            var stepsLanded = 0;

            session.StepPerformed += OnStep;
            executor.ActionFinished += OnStepFinished;

            if (card.positionMode == ActionPositionMode.MoveInFrontOfEnemy && targets.Count > 0)
            {
                executor.MoveInFrontOf(targets[0], state, BeginSession);
            }
            else
            {
                BeginSession();
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
                stepsLanded++;
                var effects = step.isFinale ? card.perfectEffects : card.effects;
                List<PerfectAnnouncementDropEffect> drops = null;
                executor.PlayAction(state, targets, ActionPositionMode.StayAtOriginalPosition, spec: step.animation, beforeAttack: BeforeAttack, onImpact: () =>
                {
                    if (drops != null)
                    {
                        foreach (var drop in drops)
                        {
                            if (drop != null)
                            {
                                drop.Land();
                            }
                        }
                    }

                    foreach (var effect in effects)
                    {
                        if (effect != null)
                        {
                            effect.Resolve(new SkillEffectContext(battleQuery, actor, targets, step.performance, step.share));
                        }
                    }
                });

                // Runs right before the swing, so the drop is timed from it.
                float BeforeAttack()
                {
                    if (!step.isFinale)
                    {
                        return 0f;
                    }

                    drops = DropPerfectAnnouncements(card, state, targets, executor, out var leadIn);
                    return leadIn;
                }
            }

            void OnStepFinished()
            {
                executor.ReturnToIdle();
            }

            void OnCompleted(SkillPerformance performance)
            {
                session.StepPerformed -= OnStep;
                _activeSession = null;
                _phaseActionState = PhaseActionState.SkillCardResolving;
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
                yield return new WaitUntil(() => !executor.IsBusy);
                Unsubscribe();

                if (notify)
                {
                    actor.NotifySkillCardResolved(battleQuery, finishedCard, finishedTargets, performance);
                }

                AnnounceTargetingClearedForSkillCard(actor, finishedCard);
                executor.ReturnToStandardPosition();
                yield return FinishSkillCardResolving(executor);
            }

            void Unsubscribe()
            {
                session.StepPerformed -= OnStep;
                executor.ActionFinished -= OnStepFinished;
            }
        }
    
        // Spawns the card's drop effect at each target, released so it
        // lands on the finale's hit event. leadIn is how long the finale must
        // wait to start when the drop is longer than its time-to-hit. Null
        // when the card has no announcement.
        private static List<PerfectAnnouncementDropEffect> DropPerfectAnnouncements(SkillCardDefinition card, CharacterState state, IReadOnlyList<ICombatTarget> targets, AttackExecutor executor, out float leadIn)
        {
            leadIn = 0f;
            if (!(card is IPerfectAnnouncementCard { PerfectAnnouncementDrop: { } template }))
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

        // Centre of the participant's Spine mesh, or its root if it has none.
        private static Vector3 CentreOf(CombatParticipant participant)
        {
            var skeleton = participant.SceneRoot.GetComponentInChildren<SkeletonRenderer>();
            if (skeleton == null || !skeleton.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                return participant.SceneTransform.position;
            }

            var bounds = meshRenderer.bounds;
            return new Vector3(bounds.center.x, bounds.center.y, participant.SceneTransform.position.z);
        }
    }
}
