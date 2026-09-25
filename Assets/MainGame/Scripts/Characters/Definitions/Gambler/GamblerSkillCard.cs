using GARA.Characters;
using GARA.SkillCards.Shake;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // A shake-input session (the "manipulate the odds" minigame) that feeds
    // its performance into a GamblingGameDefinition roll, optionally reshaped
    // by a GambleRigDefinition. Extends ShakeSkillCard rather than
    // SkillCardDefinition directly so it reuses the shake input/authoring
    // (targetPairs, scoreShaping, ...) as-is.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Gambler Skill Card", fileName = "GamblerSkillCard")]
    public class GamblerSkillCard : ShakeSkillCard
    {
        [SerializeField]
        [Expandable]
        private GamblingGameDefinition game;

        [SerializeField]
        private TierScaling manipulateByTier;

        [SerializeField]
        private bool consolationOnFail = true;

        [SerializeField]
        [Expandable]
        private GambleRigDefinition rig;

        [SerializeField]
        private GamblerSkillCard baseCard;

        public GamblingGameDefinition Game => game;
        public TierScaling ManipulateByTier => manipulateByTier;
        public bool ConsolationOnFail => consolationOnFail;
        public GambleRigDefinition Rig => rig;
        public bool IsRigged => rig != null;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new GamblerSkillInputSession(this, (ShakeSkillInputSession)base.CreateInputSession(host));
        }
    }
}
