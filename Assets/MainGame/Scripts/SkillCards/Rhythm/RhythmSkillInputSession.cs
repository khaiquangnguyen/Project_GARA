using System;
using GARA.Characters;
using GARA.Rhythm;

namespace GARA.SkillCards.Rhythm
{
    // Drives one RhythmSkillCard's minigame through the host's
    // RhythmSequencePlayer. Internal — cards construct this via
    // CreateInputSession, nothing else needs to reference it directly.
    internal sealed class RhythmSkillInputSession : ISkillInputSession
    {
        private readonly RhythmSequencePlayer _player;
        private readonly RhythmSkillCard _card;

        public RhythmSkillInputSession(RhythmSequencePlayer player, RhythmSkillCard card)
        {
            _player = player;
            _card = card;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            if (_player == null)
            {
                // Defensive: a misconfigured host (no RhythmSequencePlayer
                // driver) shouldn't hang the turn waiting for a session that
                // can never complete.
                onCompleted(SkillPerformance.Failed());
                return;
            }

            _player.Play(_card.Sequence, report => onCompleted(RhythmPerformanceMapper.Map(report, _card)));
        }

        public void Abort()
        {
            // The runner's Completed event still fires once, with
            // WasAborted = true, through the same callback wired in Begin -
            // this must not invoke onCompleted a second time itself.
            _player?.Abort();
        }
    }
}
