using System;
using GARA.Characters;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Hitting the QTE cheats the card's game; the roll gates which effects
    // land on the card's move (animationSpec).
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Gamble Card", fileName = "GambleCard")]
    public class GamblerSkillCard : SkillCardDefinition
    {
        private const string GambleGroup = "Gamble";
        private const string QteGroup = "QTE";

        [BoxGroup(GambleGroup)]
        [SerializeReference]
        [SubclassPicker]
        public GamblingGame game;

        [Tooltip("Gated on the roll; the triggered ones apply on the move's hit. None triggered = the move doesn't play.")]
        [SerializeReference]
        [SubclassPicker]
        public GambleEffect[] rollEffects = Array.Empty<GambleEffect>();

        [Tooltip("Show the roll on GambleOutcomeView and hold the Gambler's reveal pause before striking.")]
        public bool showOutcome = true;

        [Tooltip("Play this card's own QTE instead of the Gambler's shared one.")]
        [BoxGroup(QteGroup)]
        public bool overrideQte;

        [BoxGroup(QteGroup)]
        [ShowIf(nameof(overrideQte))]
        public Qte qte = Qte.Default;

        protected override bool HasCardEffects => false;

        protected override bool HasPerfectEffects => false;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new GamblerLiveSkillInputSession(host.GetDriver<QtePlayer>(), QteFor(host), host.IsPlayerControlled, host.CoroutineRunner, this, TieringOf(host), RevealSecondsOf(host));
        }

        // How many times the move plays for this roll; 0 = it doesn't.
        public virtual int StrikesFor(GambleOutcome outcome)
        {
            return 1;
        }

        public virtual float SecondsBetweenStrikes => 0f;

        // This card's QTE if it declares one, else the Gambler's.
        protected Qte QteFor(ISkillInputHost host)
        {
            if (overrideQte)
            {
                return qte;
            }

            return host.Actor is Gambler gambler ? gambler.Qte : Qte.Default;
        }

        protected static SkillPerformanceTiering TieringOf(ISkillInputHost host)
        {
            return host.Actor is Gambler gambler ? gambler.SpecialTiering : SkillPerformanceTiering.Default;
        }

        protected static float RevealSecondsOf(ISkillInputHost host)
        {
            return host.Actor is Gambler gambler ? gambler.OutcomeRevealSeconds : 0f;
        }

        protected virtual void OnValidate()
        {
            if (game == null)
            {
                Debug.LogWarning($"{name}: GamblerSkillCard has no gambling game.", this);
            }

            if (rollEffects.Length == 0)
            {
                Debug.LogWarning($"{name}: GamblerSkillCard has no roll effects — it never plays.", this);
            }

            if (game == null)
            {
                return;
            }

            foreach (var effect in rollEffects)
            {
                if (effect != null && !effect.OutcomeType.IsAssignableFrom(game.OutcomeType))
                {
                    Debug.LogWarning($"{name}: {effect.GetType().Name} reads {effect.OutcomeType.Name}, but {game.GetType().Name} rolls {game.OutcomeType.Name} — it never triggers.", this);
                }
            }
        }
    }
}
