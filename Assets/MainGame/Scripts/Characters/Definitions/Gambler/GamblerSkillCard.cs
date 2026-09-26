using System;
using GARA.Characters;
using GARA.ShakeBalance;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Shake balance score is the luck of the card's game; the roll gates
    // which effects land on the card's move (animationSpec).
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Gamble Card", fileName = "GambleCard")]
    public class GamblerSkillCard : SkillCardDefinition
    {
        private const string GambleGroup = "Gamble";

        [BoxGroup(GambleGroup)]
        [SerializeField]
        [Expandable]
        private ShakeBalanceDefinition balance;

        [Tooltip("Maps the run's 0-1 result to the 0-1 score, which is the game's luck.")]
        [BoxGroup(GambleGroup)]
        [SerializeField]
        private AnimationCurve scoreShaping = AnimationCurve.Linear(0, 0, 1, 1);

        [BoxGroup(GambleGroup)]
        [SerializeReference]
        [SubclassPicker]
        public GamblingGame game;

        [Tooltip("Gated on the roll; the triggered ones apply on the move's hit. None triggered = the move doesn't play.")]
        [SerializeReference]
        [SubclassPicker]
        public GambleEffect[] rollEffects = Array.Empty<GambleEffect>();

        public ShakeBalanceDefinition Balance => balance;

        public AnimationCurve ScoreShaping => scoreShaping;

        protected override bool HasCardEffects => false;

        protected override bool HasPerfectEffects => false;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            var tiering = host.Actor is Gambler gambler ? gambler.SpecialTiering : SkillPerformanceTiering.Default;
            return new GamblerLiveSkillInputSession(host.GetDriver<ShakeBalancePlayer>(), this, tiering);
        }

        protected virtual void OnValidate()
        {
            if (balance == null)
            {
                Debug.LogWarning($"{name}: GamblerSkillCard has no balance definition.", this);
            }

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
