using System.Collections.Generic;

namespace GARA.Combat
{
    // Placeholder shape until the ability system exists — ActionId is a free
    // string rather than an ability reference. Reaction/interrupt handling
    // (preemption, effective vs. original actor) is deferred; this only
    // covers plain "actor does X to targets" for now.
    public class QueuedAction
    {
        public CombatParticipant actor;
        public string actionId;
        public List<CombatParticipant> targets = new();
    }
}
