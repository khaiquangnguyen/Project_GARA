using System.Collections.Generic;

namespace GARA.Characters.Filmmaker
{
    // Parry stacks per recorded skill, and the copies equipped.
    public sealed class RecordingState : PassiveRuntimeState
    {
        // In the order each skill was first recorded.
        public readonly List<SkillRecording> recordings = new List<SkillRecording>();

        public readonly List<FormReplaySkillCard> equipped = new List<FormReplaySkillCard>();

        public SkillRecording RecordingOf(SkillCardDefinition card)
        {
            return recordings.Find(recording => recording.card == card);
        }

        public SkillRecording StartRecording(SkillCardDefinition card)
        {
            var recording = new SkillRecording(card);
            recordings.Add(recording);
            return recording;
        }

        public bool IsEquipped(SkillCardDefinition card)
        {
            return equipped.Exists(copy => copy.SourceCard == card);
        }

        public override void Reset()
        {
            recordings.Clear();
            equipped.Clear();
        }
    }
}
