using UnityEngine;

namespace GARA.Characters
{
    // A copy of another character's skill card, played in that character's
    // form (its prefab performs the source card's own state).
    // Built at runtime (see Create), never authored; effects apply as the
    // user of this card.
    public class FormReplaySkillCard : SkillCardDefinition
    {
        [SerializeField] private SkillCardDefinition sourceCard;
        [SerializeField] private CharacterDefinition form;
        [SerializeField] private FormReplayMode mode;
        [SerializeField] private bool oneTimeUse;

        public SkillCardDefinition SourceCard => sourceCard;
        public CharacterDefinition Form => form;
        public FormReplayMode Mode => mode;

        public override bool IsOneTimeUse => oneTimeUse;

        public override bool ImpactOnEveryHit => sourceCard != null && sourceCard.ImpactOnEveryHit;

        // Null when form has no state playing sourceCard.
        public static FormReplaySkillCard Create(SkillCardDefinition sourceCard, CharacterDefinition form, FormReplayMode mode, bool oneTimeUse)
        {
            if (sourceCard == null || form == null || form.FindSkillCardState(sourceCard) == null)
            {
                return null;
            }

            var card = CreateInstance<FormReplaySkillCard>();
            card.hideFlags = HideFlags.DontSave;
            card.name = $"Replay_{sourceCard.name}";
            card.sourceCard = sourceCard;
            card.form = form;
            card.mode = mode;
            card.oneTimeUse = oneTimeUse;

            card.cardId = $"replay_{sourceCard.cardId}";
            card.displayName = oneTimeUse ? $"Replay: {sourceCard.displayName}" : sourceCard.displayName;
            card.description = sourceCard.description;
            card.icon = sourceCard.icon != null ? sourceCard.icon : form.portrait;
            card.targetMode = sourceCard.targetMode;
            card.multiTargetCount = sourceCard.multiTargetCount;
            card.stepTargeting = sourceCard.stepTargeting;
            card.teleportBetweenSteps = sourceCard.teleportBetweenSteps;
            card.alliesJoinFinale = sourceCard.alliesJoinFinale;
            card.allyDanceDuration = sourceCard.allyDanceDuration;
            card.finaleTargeting = sourceCard.finaleTargeting;
            card.positionMode = sourceCard.positionMode;
            card.finalePositionMode = sourceCard.finalePositionMode;
            card.effects = sourceCard.effects;
            card.perfectEffects = sourceCard.perfectEffects;
            card.endDelay = sourceCard.endDelay;
            card.finaleDelay = sourceCard.finaleDelay;
            card.animationSpec = sourceCard.animationSpec;
            return card;
        }

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return sourceCard.CreateInputSession(host);
        }
    }
}
