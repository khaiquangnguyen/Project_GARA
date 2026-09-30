using GARA.Characters;
using TMPro;
using UnityEngine;

namespace GARA.Combat
{
    // Shows a passive's card offer (see CombatSceneManager.SkillCardOffer):
    // the offered card and one label per choice, the current one
    // highlighted. Labels past the offer's choices are hidden.
    public class SkillCardOfferView : MonoBehaviour
    {
        [Tooltip("Shown only while a card is on offer. A child, not this object, so it keeps listening.")]
        [SerializeField] private GameObject root;

        [SerializeField] private SkillCardSlot offeredCard;

        [Tooltip("Optional. Names the offered card.")]
        [SerializeField] private TMP_Text title;

        [SerializeField] private TMP_Text[] choiceLabels = new TMP_Text[3];

        [SerializeField] private Color choiceColor = Color.white;

        [SerializeField] private Color highlightedChoiceColor = Color.green;

        private void Awake()
        {
            SetVisible(false);
        }

        private void OnEnable()
        {
            CombatSceneManager.SkillCardOfferStarted += OnOfferStarted;
            CombatSceneManager.SkillCardOfferChoiceChanged += OnChoiceChanged;
            CombatSceneManager.SkillCardOfferEnded += OnOfferEnded;
        }

        private void OnDisable()
        {
            CombatSceneManager.SkillCardOfferStarted -= OnOfferStarted;
            CombatSceneManager.SkillCardOfferChoiceChanged -= OnChoiceChanged;
            CombatSceneManager.SkillCardOfferEnded -= OnOfferEnded;
        }

        private void OnOfferStarted(SkillCardOffer offer)
        {
            if (offeredCard != null)
            {
                offeredCard.Show(offer.card);
            }

            if (title != null)
            {
                title.text = $"Recorded: {NameOf(offer.card)}";
            }

            for (var i = 0; i < choiceLabels.Length; i++)
            {
                if (choiceLabels[i] == null)
                {
                    continue;
                }

                var shown = i < offer.choices.Count;
                choiceLabels[i].gameObject.SetActive(shown);
                if (shown)
                {
                    choiceLabels[i].text = LabelOf(offer.choices[i]);
                }
            }

            SetVisible(true);
        }

        private void OnChoiceChanged(int index)
        {
            for (var i = 0; i < choiceLabels.Length; i++)
            {
                if (choiceLabels[i] != null)
                {
                    choiceLabels[i].color = i == index ? highlightedChoiceColor : choiceColor;
                }
            }
        }

        private void OnOfferEnded()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        private static string LabelOf(SkillCardOfferChoice choice)
        {
            if (!choice.accept)
            {
                return "Skip";
            }

            return choice.replaced != null ? $"Replace {NameOf(choice.replaced)}" : "Equip";
        }

        private static string NameOf(SkillCardDefinition card)
        {
            return string.IsNullOrEmpty(card.displayName) ? card.name : card.displayName;
        }
    }
}
