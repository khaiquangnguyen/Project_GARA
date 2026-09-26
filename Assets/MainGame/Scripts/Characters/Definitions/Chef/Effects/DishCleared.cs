using GARA.InputSets;

namespace GARA.Characters.Chef
{
    internal static class DishCleared
    {
        // Shared gate of the dish effects: something got cooked.
        public static bool AnyStep(InputSetCompletionReport report)
        {
            return report.ClearedSets > 0;
        }
    }
}
