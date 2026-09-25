using System;
using GARA.Characters;
using GARA.ShakeBalance;

namespace GARA.SkillCards.ShakeBalance
{
    // Adapts one ShakeBalancePlayer run to ISkillInputSession. Begin/Abort
    // just forward to the player; ShakeBalancePlayer.Abort() already
    // delivers a real (Aborted) ShakeBalanceReport through the same
    // onCompleted callback Play() was given, so no separate
    // SkillPerformance.Failed() synthesis is needed here.
    public sealed class ShakeBalanceSkillInputSession : ISkillInputSession
    {
        private readonly ShakeBalanceSkillCard _card;
        private readonly ShakeBalancePlayer _player;

        public ShakeBalanceSkillInputSession(ShakeBalanceSkillCard card, ShakeBalancePlayer player)
        {
            _card = card;
            _player = player;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            if (_player == null || _card.Balance == null)
            {
                onCompleted?.Invoke(SkillPerformance.Failed());
                return;
            }

            _player.Play(_card.Balance, report => onCompleted?.Invoke(_card.BuildPerformance(report)));
        }

        public void Abort()
        {
            _player?.Abort();
        }
    }
}
