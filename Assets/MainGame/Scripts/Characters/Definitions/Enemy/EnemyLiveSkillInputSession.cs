using System;
using System.Collections.Generic;
using GARA.Characters;

namespace GARA.Characters.Enemy
{
    // Emits the card's one move with every effect, as a Perfect.
    internal sealed class EnemyLiveSkillInputSession : ILiveSkillInputSession
    {
        private readonly EnemySkillCard _card;

        public event Action<SkillStep> StepPerformed;

        public AttackAnimationSpec OpeningMove => _card.animationSpec;

        public EnemyLiveSkillInputSession(EnemySkillCard card)
        {
            _card = card;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            var performance = new SkillPerformance(1f, SkillPerformanceTier.Perfect, false, null);
            var effects = new List<ISkillEffect>();
            foreach (var effect in _card.hitEffects)
            {
                if (effect != null)
                {
                    effects.Add(effect);
                }
            }

            if (effects.Count > 0)
            {
                StepPerformed?.Invoke(new SkillStep(_card.animationSpec, performance, effects, false));
            }

            onCompleted(performance);
        }

        public void Abort()
        {
        }
    }
}
