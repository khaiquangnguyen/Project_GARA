using System;
using GARA.Characters;
using GARA.SkillCards.Shake;

namespace GARA.Characters.Gambler
{
    // Wraps a ShakeSkillInputSession: lets the shake minigame run as normal
    // to produce a "manipulate the odds" SkillPerformance, then rolls the
    // card's GamblingGameDefinition/GambleRigDefinition against that
    // performance via GambleResolver before handing the final,
    // gamble-flavoured SkillPerformance up to the caller.
    public sealed class GamblerSkillInputSession : ISkillInputSession
    {
        private readonly GamblerSkillCard _card;
        private readonly ShakeSkillInputSession _inner;

        public GamblerSkillInputSession(GamblerSkillCard card, ShakeSkillInputSession inner)
        {
            _card = card;
            _inner = inner;
        }

        public event Action<GambleResolution> GambleResolved;

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            _inner.Begin(manipulationPerformance =>
            {
                var shakeReport = manipulationPerformance.TryGetDetails<ShakeSkillReport>(out var report) ? report : null;
                var rng = new UnityGambleRandom();
                var final = GambleResolver.Resolve(_card, manipulationPerformance, shakeReport, rng);

                if (final.TryGetDetails<GambleResolution>(out var resolution))
                {
                    GambleResolved?.Invoke(resolution);
                }

                onCompleted(final);
            });
        }

        public void Abort()
        {
            _inner.Abort();
        }
    }
}
