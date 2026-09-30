using System.Collections.Generic;

namespace GARA.Characters
{
    // A skill card a passive offers its owner as their turn starts: taken
    // into a free slot, swapped for an equipped card, or skipped.
    public readonly struct SkillCardOffer
    {
        public readonly PassiveDefinition passive;
        public readonly SkillCardDefinition card;
        public readonly IReadOnlyList<SkillCardDefinition> equipped;
        public readonly bool hasFreeSlot;

        // Equip (or one Replace per equipped card when full), then Decline.
        public readonly IReadOnlyList<SkillCardOfferChoice> choices;

        public SkillCardOffer(PassiveDefinition passive, SkillCardDefinition card, IReadOnlyList<SkillCardDefinition> equipped, bool hasFreeSlot)
        {
            this.passive = passive;
            this.card = card;
            this.equipped = equipped;
            this.hasFreeSlot = hasFreeSlot;

            var choices = new List<SkillCardOfferChoice>();
            if (hasFreeSlot)
            {
                choices.Add(SkillCardOfferChoice.Equip);
            }
            else
            {
                foreach (var equippedCard in equipped)
                {
                    choices.Add(SkillCardOfferChoice.Replace(equippedCard));
                }
            }

            choices.Add(SkillCardOfferChoice.Decline);
            this.choices = choices;
        }
    }
}
