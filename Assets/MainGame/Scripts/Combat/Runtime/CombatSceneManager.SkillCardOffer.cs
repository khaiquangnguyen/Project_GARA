using System;
using System.Collections.Generic;
using GARA.Characters;

namespace GARA.Combat
{
    // Card offers from the actor's passives (e.g. the Filmmaker's recorded
    // skills), answered one at a time before a player's turn opens. A/D pick
    // a choice, Enter confirms, Esc skips. AI actors only ever take a free
    // slot.
    public partial class CombatSceneManager
    {
        public static event Action<SkillCardOffer> SkillCardOfferStarted;

        // Index into the offer's choices.
        public static event Action<int> SkillCardOfferChoiceChanged;

        public static event Action SkillCardOfferEnded;

        // Each card is offered at most once per turn.
        private readonly List<SkillCardDefinition> _offeredSkillCards = new();
        private SkillCardOffer _offer;
        private int _offerChoiceIndex;

        public bool IsOfferingSkillCard => _phaseActionState == PhaseActionState.SkillCardOffer;

        public bool MoveSkillCardOfferChoice(int step)
        {
            if (!IsOfferingSkillCard)
            {
                return false;
            }

            var count = _offer.choices.Count;
            _offerChoiceIndex = ((_offerChoiceIndex + step) % count + count) % count;
            SkillCardOfferChoiceChanged?.Invoke(_offerChoiceIndex);
            return true;
        }

        public bool ConfirmSkillCardOffer()
        {
            return IsOfferingSkillCard && AnswerSkillCardOffer(_offer.choices[_offerChoiceIndex]);
        }

        public bool DeclineSkillCardOffer()
        {
            return IsOfferingSkillCard && AnswerSkillCardOffer(SkillCardOfferChoice.Decline);
        }

        private bool TryOfferNextSkillCard()
        {
            if (!_actor.Passives.TryGetSkillCardOffer(_actor, _offeredSkillCards, out var offer))
            {
                return false;
            }

            _offeredSkillCards.Add(offer.card);
            _offer = offer;
            _offerChoiceIndex = 0;
            _phaseActionState = PhaseActionState.SkillCardOffer;
            SkillCardOfferStarted?.Invoke(offer);
            SkillCardOfferChoiceChanged?.Invoke(_offerChoiceIndex);
            return true;
        }

        private bool AnswerSkillCardOffer(SkillCardOfferChoice choice)
        {
            var offer = _offer;
            EndSkillCardOffer();
            _actor.Passives.ResolveSkillCardOffer(_actor, in offer, choice);
            RefreshHand();

            if (!TryOfferNextSkillCard())
            {
                ResetCardHighlight();
            }

            return true;
        }

        // Leaves an unanswered offer pending for a later turn.
        private void EndSkillCardOffer()
        {
            if (!IsOfferingSkillCard)
            {
                return;
            }

            _offer = default;
            _phaseActionState = PhaseActionState.Regular;
            SkillCardOfferEnded?.Invoke();
        }

        private void ResolveAiSkillCardOffers()
        {
            while (_actor.Passives.TryGetSkillCardOffer(_actor, _offeredSkillCards, out var offer))
            {
                _offeredSkillCards.Add(offer.card);
                var choice = offer.hasFreeSlot ? SkillCardOfferChoice.Equip : SkillCardOfferChoice.Decline;
                _actor.Passives.ResolveSkillCardOffer(_actor, in offer, choice);
            }
        }
    }
}
