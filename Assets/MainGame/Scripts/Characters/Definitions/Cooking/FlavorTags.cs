using System.Collections.Generic;

namespace GARA.Characters
{
    public static class FlavorTags
    {
        public const FlavorTag Visible = FlavorTag.Spicy | FlavorTag.Sweet | FlavorTag.Sour | FlavorTag.Bitter;

        public const FlavorTag All = Visible | FlavorTag.Umami;

        public static FlavorTag VisibleOnly(this FlavorTag tag)
        {
            return tag & Visible;
        }

        // Each single flavor set in tag, one flag at a time.
        public static IEnumerable<FlavorTag> Split(this FlavorTag tag)
        {
            for (var bit = 1; bit <= (int)All; bit <<= 1)
            {
                var flavor = (FlavorTag)bit;
                if ((tag & flavor & All) != 0)
                {
                    yield return flavor;
                }
            }
        }
    }
}
