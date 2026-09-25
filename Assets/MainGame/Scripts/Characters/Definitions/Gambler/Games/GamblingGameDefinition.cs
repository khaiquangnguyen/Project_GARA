using UnityEngine;

namespace GARA.Characters.Gambler
{
    // One authored gambling minigame's resolution rules — pure logic, no
    // input/animation concerns. GamblerSkillCard.Game picks one of these;
    // GambleResolver drives it.
    public abstract class GamblingGameDefinition : ScriptableObject
    {
        public abstract GamblingGameType GameType { get; }

        public abstract GambleOutcome Resolve(GambleContext context, IGambleRandom rng);

        public abstract float PredictSuccessChance(float manipulate);
    }
}
