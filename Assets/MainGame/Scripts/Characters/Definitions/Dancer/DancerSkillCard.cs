using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    [CreateAssetMenu(menuName = "GARA/Characters/Dancer/Dance Card", fileName = "DanceCard")]
    public class DancerSkillCard : RhythmSkillCard
    {
        [SerializeField]
        private DanceType danceType;

        public DanceType DanceType => danceType;

        protected virtual void OnValidate()
        {
            base.OnValidate();
        }
    }
}
