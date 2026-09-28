using GARA.Characters;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The Gambler and the target take turns on one revolver until someone
    // is shot or chickens out. Each pull plays as its own move; the ending's
    // move carries the roll effects. An empty spec plays animationSpec.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Duel Roulette Card", fileName = "DuelRouletteCard")]
    public class DuelRouletteSkillCard : GamblerSkillCard
    {
        private const string DuelGroup = "Duel";

        [Tooltip("Seconds an empty click gets before the next pull.")]
        [BoxGroup(DuelGroup)]
        [Min(0)]
        public float secondsPerPull = 0.8f;

        [BoxGroup(DuelGroup)]
        public AttackAnimationSpec gamblerPullSpec;

        [BoxGroup(DuelGroup)]
        public AttackAnimationSpec gamblerShotSpec;

        [BoxGroup(DuelGroup)]
        public AttackAnimationSpec gamblerChickenOutSpec;

        [BoxGroup(DuelGroup)]
        public AttackAnimationSpec targetPullSpec;

        [BoxGroup(DuelGroup)]
        public AttackAnimationSpec targetShotSpec;

        [BoxGroup(DuelGroup)]
        public AttackAnimationSpec targetChickenOutSpec;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new DuelRouletteLiveSkillInputSession(host.GetDriver<CheatShakePlayer>(), CheatShakeOf(host), host.GetDriver<DuelRouletteDecisionPlayer>(), host.CoroutineRunner, this, TieringOf(host), host.IsPlayerControlled);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (game != null && !(game is DuelRouletteGame))
            {
                Debug.LogWarning($"{name}: DuelRouletteSkillCard needs a {nameof(DuelRouletteGame)}.", this);
            }
        }
    }
}
