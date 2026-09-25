using System;
using GARA.Characters;
using GARA.InputSets;

namespace GARA.SkillCards.InputSets
{
    // Adapts one InputSetCollectionPlayer run to ISkillInputSession. Begin/
    // Abort just forward to the player; InputSetCollectionPlayer.Abort()
    // already delivers a real (WasAborted = true) InputSetCompletionReport
    // through the same onCompleted callback Play() was given, so no
    // separate SkillPerformance.Failed() synthesis is needed here.
    public sealed class InputSetSkillInputSession : ISkillInputSession
    {
        private readonly InputSetSkillCard _card;
        private readonly InputSetCollectionPlayer _player;

        public InputSetSkillInputSession(InputSetSkillCard card, InputSetCollectionPlayer player)
        {
            _card = card;
            _player = player;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            if (_player == null)
            {
                onCompleted?.Invoke(SkillPerformance.Failed());
                return;
            }

            _player.Play(_card.SetCollection, report => onCompleted?.Invoke(_card.BuildPerformance(report)));
        }

        public void Abort()
        {
            _player?.Abort();
        }
    }
}
