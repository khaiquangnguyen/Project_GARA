using UnityEngine;

namespace GARA.Characters.FrogBard
{
    // Frog Bard passive: the Bard is always in one emotion, which decides how
    // each special plays (see DualEmotionEffect). Every special it finishes
    // swings it to the other emotion.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Mood Swing", fileName = "MoodSwingPassive")]
    public class MoodSwingPassive : PassiveDefinition<MoodSwingState>
    {
        [SerializeField] private Emotion startingEmotion = Emotion.Joy;

        [Tooltip("Whether an aborted special still swings the emotion.")]
        [SerializeField] private bool swingOnAbort;

        // Joy when character has no Mood Swing passive.
        public static Emotion EmotionOf(ICombatTarget character)
        {
            var passives = character?.Passives;
            if (passives == null)
            {
                return Emotion.Joy;
            }

            foreach (var passive in passives.Passives)
            {
                if (passive is MoodSwingPassive moodSwing && passives.GetState(passive) is MoodSwingState state)
                {
                    return moodSwing.CurrentOf(state);
                }
            }

            return Emotion.Joy;
        }

        public Emotion CurrentOf(MoodSwingState state)
        {
            return state.current ?? startingEmotion;
        }

        protected override void OnSkillCardResolved(in PassiveContext context, MoodSwingState state)
        {
            if (!(context.Card is FrogBardSkillCard))
            {
                return;
            }

            if (context.Performance.WasAborted && !swingOnAbort)
            {
                return;
            }

            state.current = Next(CurrentOf(state));
            Debug.Log($"[{nameof(MoodSwingPassive)}] mood swings to {state.current}.");
        }

        private static Emotion Next(Emotion emotion)
        {
            return emotion == Emotion.Joy ? Emotion.Sadness : Emotion.Joy;
        }
    }
}
