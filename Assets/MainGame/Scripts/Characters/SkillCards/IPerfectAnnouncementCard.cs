using UnityEngine;

namespace GARA.Characters
{
    // A card whose perfect finale drops an announcement onto each target,
    // landing on the finale's hit frame.
    public interface IPerfectAnnouncementCard
    {
        // This card's own drop prefab (holds a PerfectAnnouncementDropEffect).
        GameObject PerfectAnnouncementDrop { get; }
    }
}
