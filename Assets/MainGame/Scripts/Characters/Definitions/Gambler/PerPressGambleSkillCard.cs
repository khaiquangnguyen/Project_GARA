using GARA.Characters;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Plays a run of QTEs; each hit plays the move at once with one press of
    // the game (an IPerPressGame). A miss plays nothing, so the press waits
    // for the next hit. See PerPressGambleLiveSkillInputSession.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Per Press Gamble Card", fileName = "PerPressGambleCard")]
    public class PerPressGambleSkillCard : GamblerSkillCard
    {
        private const string PressGroup = "Per Press";

        [Tooltip("QTEs in the run; 0 = the game's press cap.")]
        [BoxGroup(PressGroup)]
        [Min(0)]
        public int qteCount;

        public int QteCountFor(IPerPressGame perPress)
        {
            return qteCount > 0 ? qteCount : perPress.MaxPresses;
        }

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new PerPressGambleLiveSkillInputSession(host.GetDriver<QtePlayer>(), QteFor(host), host.IsPlayerControlled, this, TieringOf(host));
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (game == null)
            {
                return;
            }

            if (!(game is IPerPressGame perPress))
            {
                Debug.LogWarning($"{name}: PerPressGambleSkillCard needs an {nameof(IPerPressGame)} game.", this);
                return;
            }

            if (QteCountFor(perPress) <= 0)
            {
                Debug.LogWarning($"{name}: {game.GetType().Name} has no press cap, so it needs a QTE count.", this);
            }
        }
    }
}
