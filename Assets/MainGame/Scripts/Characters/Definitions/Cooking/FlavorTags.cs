namespace GARA.Characters
{
    // Extension helpers for FlavorTag — keeps the "Umami is hidden" rule in
    // one place instead of every caller re-deriving it with bitmasks.
    public static class FlavorTags
    {
        public const FlavorTag Visible = FlavorTag.Spicy | FlavorTag.Sweet | FlavorTag.Sour | FlavorTag.Bitter;

        public static FlavorTag VisibleOnly(this FlavorTag tag)
        {
            return tag & Visible;
        }

        public static bool HasUmami(this FlavorTag tag)
        {
            return (tag & FlavorTag.Umami) != 0;
        }

        public static int CountMatches(this FlavorTag dish, FlavorTag palate)
        {
            var overlap = dish & palate & Visible;
            return CountFlavors(overlap);
        }

        public static int CountFlavors(this FlavorTag tag)
        {
            var count = 0;
            var value = (int)tag;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }
    }
}
