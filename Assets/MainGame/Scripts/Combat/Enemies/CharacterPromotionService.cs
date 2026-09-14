using System;
using GARA.Characters;

namespace GARA.Combat
{
    // Converts a defeated/recruited enemy CombatParticipant into a persistent
    // ManagedCharacter. Called post-battle only (never mid-round) so it can't
    // disturb a BattleContext that's still resolving. Only builds the object —
    // persisting it is the caller's job, keeping GARA.Combat free of any
    // dependency on the save layer.
    public static class CharacterPromotionService
    {
        public static bool ShouldPromote(CombatParticipant participant)
        {
            return participant.enemySource != null
                   && participant.enemySource.promotionPolicy != PromotionPolicy.None;
        }

        public static ManagedCharacter Promote(CombatParticipant participant)
        {
            if (!ShouldPromote(participant))
            {
                throw new InvalidOperationException($"{participant.participantId} is not eligible for promotion.");
            }

            var definition = participant.definition;
            var managedCharacter = new ManagedCharacter(Guid.NewGuid().ToString(), definition.characterId)
            {
                level = participant.enemySource.level
            };

            return managedCharacter;
        }
    }
}
