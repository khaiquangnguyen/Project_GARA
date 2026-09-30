using UnityEngine;

namespace GARA.Characters.Bard
{
    // Bard passive: the Bard is always in one emotion, which decides how
    // each special plays (see DualEmotionEffect). Cards change it through
    // MoodChangeEffect.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Mood Swing", fileName = "MoodSwingPassive")]
    public class MoodSwingPassive : PassiveDefinition<MoodSwingState>
    {
        [SerializeField] private Emotion startingEmotion = Emotion.Joy;

        // Joy when character has no Mood Swing passive.
        public static Emotion EmotionOf(ICombatTarget character)
        {
            return TryFind(character, out var moodSwing, out var state) ? moodSwing.CurrentOf(state) : Emotion.Joy;
        }

        // No-op when character has no Mood Swing passive.
        public static void ChangeMood(ICombatTarget character, MoodChange change)
        {
            if (!TryFind(character, out var moodSwing, out var state))
            {
                return;
            }

            var current = moodSwing.CurrentOf(state);
            state.current = change switch
            {
                MoodChange.ToJoy => Emotion.Joy,
                MoodChange.ToSadness => Emotion.Sadness,
                _ => current == Emotion.Joy ? Emotion.Sadness : Emotion.Joy
            };
            Debug.Log($"[{nameof(MoodSwingPassive)}] mood is now {state.current}.");
        }

        public Emotion CurrentOf(MoodSwingState state)
        {
            return state.current ?? startingEmotion;
        }

        protected override void OnSkillCardResolved(in PassiveContext context, MoodSwingState state)
        {
        }

        private static bool TryFind(ICombatTarget character, out MoodSwingPassive moodSwing, out MoodSwingState state)
        {
            var passives = character?.Passives;
            if (passives != null)
            {
                foreach (var passive in passives.Passives)
                {
                    if (passive is MoodSwingPassive found && passives.GetState(passive) is MoodSwingState foundState)
                    {
                        moodSwing = found;
                        state = foundState;
                        return true;
                    }
                }
            }

            moodSwing = null;
            state = null;
            return false;
        }
    }
}
