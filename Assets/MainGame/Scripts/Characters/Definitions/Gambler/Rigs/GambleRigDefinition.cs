using UnityEngine;

namespace GARA.Characters.Gambler
{
    // An authored "cheat" attached to a GamblerSkillCard: nudges the context
    // before a game rolls (BeforeResolve) and/or reshapes the outcome after
    // (AfterResolve). Both hooks are optional no-ops by default so a rig can
    // implement just the one it needs.
    public abstract class GambleRigDefinition : ScriptableObject
    {
        [SerializeField]
        private string rigName;

        [SerializeField, TextArea]
        private string flavor;

        [SerializeField]
        private GamblingGameType[] applicableTo;

        public string RigName => rigName;
        public string Flavor => flavor;
        public GamblingGameType[] ApplicableTo => applicableTo;

        public virtual void BeforeResolve(GambleContext context)
        {
        }

        public virtual void AfterResolve(GambleContext context, ref GambleOutcome outcome)
        {
        }
    }
}
