using System;
using System.Collections.Generic;
using GARA.Characters;
using GARA.InputSets;
using GARA.Rhythm;
using GARA.ShakeBalance;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Combat
{
    // Presentation of the combat: the turn-order strip, the turn/special
    // announcements, the per-character targeting effects, and the minigame
    // visual drivers. Each slot's own TurnAvatar (and each announcement's
    // own GameObject) owns how it actually renders — this part only drives
    // them, so any individual visual can change without this needing to.
    // The visual drivers are fed by any player's static started/ended
    // events, so whoever plays a minigame (a character's skill card, or the
    // Combat Scene Development window) is presented the same way.
    public partial class CombatSceneManager
    {
        [Header("Overlay")]
        [Tooltip("Exactly 6 slots, left to right — slot 0 is always the current turn, since the queue itself shifts left as turns are consumed.")]
        [SerializeField] private TurnAvatar[] turnOrderSlots = new TurnAvatar[6];

        [Tooltip("Shows the acting player's combat loadout, in order.")]
        [SerializeField] private SkillCardSlot[] skillCardSlots = new SkillCardSlot[4];

        [Tooltip("Shown while a player character is acting, hidden otherwise.")]
        [SerializeField] private GameObject playerTurnAnnouncement;

        [Tooltip("Shown while an enemy is acting, hidden otherwise.")]
        [SerializeField] private GameObject enemyTurnAnnouncement;

        // Per-character effect templates, one set per side — each taken
        // from that side's EffectDummy (PlayerEffectDummy/EnemyEffectDummy)
        // and cloned onto every character of that faction once combat
        // starts, so player and enemy characters can react differently.
        [Header("Player Character Effects")]
        [Tooltip("PlayerEffectDummy's OnNotTargetedEffect child.")]
        [FormerlySerializedAs("notTargetedEffectTemplate")]
        [SerializeField] private GameObject playerNotTargetedEffectTemplate;

        [Tooltip("PlayerEffectDummy's OnNotTargetedShrinkEffect child.")]
        [FormerlySerializedAs("notTargetedShrinkEffectTemplate")]
        [SerializeField] private GameObject playerNotTargetedShrinkEffectTemplate;

        [Tooltip("PlayerEffectDummy's OnHitEffect child.")]
        [FormerlySerializedAs("hitEffectTemplate")]
        [SerializeField] private GameObject playerHitEffectTemplate;

        [Tooltip("PlayerEffectDummy's OnParrySuccessEffect child.")]
        [SerializeField] private GameObject playerParrySuccessEffectTemplate;

        [Tooltip("PlayerEffectDummy's OnJumpSuccessEffect child.")]
        [SerializeField] private GameObject playerJumpSuccessEffectTemplate;

        [Header("Enemy Character Effects")]
        [Tooltip("EnemyEffectDummy's OnNotTargetedEffect child.")]
        [SerializeField] private GameObject enemyNotTargetedEffectTemplate;

        [Tooltip("EnemyEffectDummy's OnNotTargetedShrinkEffect child.")]
        [SerializeField] private GameObject enemyNotTargetedShrinkEffectTemplate;

        [Tooltip("EnemyEffectDummy's OnHitEffect child.")]
        [SerializeField] private GameObject enemyHitEffectTemplate;

        [Tooltip("EnemyEffectDummy's OnParrySuccessEffect child.")]
        [SerializeField] private GameObject enemyParrySuccessEffectTemplate;

        [Tooltip("EnemyEffectDummy's OnJumpSuccessEffect child.")]
        [SerializeField] private GameObject enemyJumpSuccessEffectTemplate;

        [Header("Enemy HP")]
        [Tooltip("Spawned on each enemy's HP heart anchor at battle start; only shown while that enemy is targeted.")]
        [SerializeField] private HpHeartView enemyHpHeartPrefab;

        [Header("Minigame Visual Drivers")]

        [Tooltip("Scene instance of the RhythmVisualDriver prefab — shown whenever any rhythm sequence starts.")]
        [SerializeField] private RhythmVisualDriver rhythmVisualDriver;

        [Tooltip("Scene instance of the InputSetVisualDriver prefab — shown whenever any input set collection starts.")]
        [SerializeField] private InputSetVisualDriver inputSetVisualDriver;

        [Tooltip("Scene instance of the ShakeBalanceVisualDriver prefab — shown whenever any shake balance run starts.")]
        [SerializeField] private ShakeBalanceVisualDriver shakeBalanceVisualDriver;

        // The ViewShake prefab's shakers, which move the whole rendered view.
        // They hold no shake settings of their own: whoever shakes the view
        // sends them — a Position / Rotation Shake feedback on channel 10 with
        // no target shaker (from any prefab), or the Combat Scene Development
        // window's View Shake test, which calls them directly.
        [Header("View Shake")]
        [SerializeField] private MMPositionShaker viewPositionShaker;
        [SerializeField] private MMRotationShaker viewRotationShaker;

        private bool _hasClonedCharacterEffects;

        private readonly Dictionary<CombatParticipant, HpHeartView> _hpHearts = new();

        private void EnableOverlay()
        {
            RhythmSequencePlayer.AnySequenceStarted += ShowRhythmSequence;
            RhythmSequencePlayer.AnySequenceEnded += HideRhythmSequence;
            InputSetCollectionPlayer.AnySetCollectionStarted += ShowInputSetCollection;
            InputSetCollectionPlayer.AnySetCollectionEnded += HideInputSetCollection;
            ShakeBalancePlayer.AnyBalanceStarted += ShowShakeBalance;
            ShakeBalancePlayer.AnyBalanceEnded += HideShakeBalance;
            CombatParticipant.HpChanged += RefreshHpHeart;
        }

        private void DisableOverlay()
        {
            RhythmSequencePlayer.AnySequenceStarted -= ShowRhythmSequence;
            RhythmSequencePlayer.AnySequenceEnded -= HideRhythmSequence;
            InputSetCollectionPlayer.AnySetCollectionStarted -= ShowInputSetCollection;
            InputSetCollectionPlayer.AnySetCollectionEnded -= HideInputSetCollection;
            ShakeBalancePlayer.AnyBalanceStarted -= ShowShakeBalance;
            ShakeBalancePlayer.AnyBalanceEnded -= HideShakeBalance;
            CombatParticipant.HpChanged -= RefreshHpHeart;
        }

        private void RefreshTurnOrder(Sprite[] portraits)
        {
            // First time the turn order is populated, every character has
            // definitely finished spawning (BeginBattle only runs once
            // CombatManager's GenerateParticipants is fully done) — the
            // reliable moment to clone the per-character effects onto each
            // of them.
            if (!_hasClonedCharacterEffects)
            {
                // The fade and shrink are just not-targeted effects reacting
                // to TargetedStateEvent — leaving one off every character
                // turns it off, while the event itself still broadcasts.
                if (fadeIfNotTargeted)
                {
                    CloneEffectOntoEveryCharacter(playerNotTargetedEffectTemplate, enemyNotTargetedEffectTemplate);
                }

                if (shrinkIfNotTargeted)
                {
                    CloneEffectOntoEveryCharacter(playerNotTargetedShrinkEffectTemplate, enemyNotTargetedShrinkEffectTemplate);
                }

                CloneEffectOntoEveryCharacter(playerHitEffectTemplate, enemyHitEffectTemplate);
                CloneEffectOntoEveryCharacter(playerParrySuccessEffectTemplate, enemyParrySuccessEffectTemplate);
                CloneEffectOntoEveryCharacter(playerJumpSuccessEffectTemplate, enemyJumpSuccessEffectTemplate);
                SpawnEnemyHpHearts();
                _hasClonedCharacterEffects = true;
            }

            for (var i = 0; i < turnOrderSlots.Length; i++)
            {
                var slot = turnOrderSlots[i];
                if (slot == null)
                {
                    continue;
                }

                slot.SetPortrait(i < portraits.Length ? portraits[i] : null);
                slot.SetCurrent(i == 0);
            }
        }

        // Each character gets its own side's template — a side with none
        // assigned simply goes without that effect.
        private void CloneEffectOntoEveryCharacter(GameObject playerTemplate, GameObject enemyTemplate)
        {
            foreach (var (participant, executor) in _executors)
            {
                var effectTemplate = participant.faction == FactionTag.Player ? playerTemplate : enemyTemplate;
                if (effectTemplate != null)
                {
                    Instantiate(effectTemplate, executor.transform, false);
                }
            }
        }

        // Parented to the anchor so the heart follows its enemy around.
        private void SpawnEnemyHpHearts()
        {
            if (enemyHpHeartPrefab == null)
            {
                return;
            }

            foreach (var (participant, executor) in _executors)
            {
                if (participant.faction != FactionTag.Enemy)
                {
                    continue;
                }

                var definition = executor.GetComponent<CharacterDefinition>();
                var anchor = definition != null ? definition.hpHeartAnchor : null;
                if (anchor == null)
                {
                    Debug.LogWarning($"{executor.name} has no hpHeartAnchor; skipping its HP heart.", executor);
                    continue;
                }

                var heart = Instantiate(enemyHpHeartPrefab, anchor, false);
                _hpHearts[participant] = heart;
                RefreshHpHeart(participant);
                heart.gameObject.SetActive(false);
            }
        }

        // Shows the per-character target displays (the HP heart for now) on
        // exactly these participants and hides them everywhere else. New
        // target-only displays get toggled here too.
        private void ShowTargetDisplays(IEnumerable<CombatParticipant> targets)
        {
            var shown = new HashSet<CombatParticipant>(targets);
            foreach (var (participant, heart) in _hpHearts)
            {
                if (heart != null)
                {
                    heart.gameObject.SetActive(shown.Contains(participant));
                }
            }
        }

        private void HideTargetDisplays()
        {
            ShowTargetDisplays(Array.Empty<CombatParticipant>());
        }

        private void RefreshHpHeart(CombatParticipant participant)
        {
            if (_hpHearts.TryGetValue(participant, out var heart) && heart != null)
            {
                heart.SetHp(participant.currentHp, participant.GetCurrentStats().MaxHp.Value);
            }
        }

        // Empty on non-player turns.
        private void RefreshSkillCardSlots(CombatParticipant actor)
        {
            var cards = actor != null && actor.faction == FactionTag.Player ? actor.SkillCards : null;
            for (var i = 0; i < skillCardSlots.Length; i++)
            {
                var slot = skillCardSlots[i];
                if (slot == null)
                {
                    continue;
                }

                if (cards != null && i < cards.Count)
                {
                    slot.Show(cards[i]);
                }
                else
                {
                    slot.Clear();
                }

                slot.SetHighlighted(false);
            }
        }

        // -1 highlights none.
        private void HighlightSkillCardSlot(int index)
        {
            for (var i = 0; i < skillCardSlots.Length; i++)
            {
                if (skillCardSlots[i] != null)
                {
                    skillCardSlots[i].SetHighlighted(i == index);
                }
            }
        }

        private void ToggleTurnAnnouncement(bool isPlayerTurn)
        {
            if (playerTurnAnnouncement != null)
            {
                playerTurnAnnouncement.SetActive(isPlayerTurn);
            }

            if (enemyTurnAnnouncement != null)
            {
                enemyTurnAnnouncement.SetActive(!isPlayerTurn);
            }
        }

        private void ShowRhythmSequence(RhythmSequenceRunner runner)
        {
            if (rhythmVisualDriver != null)
            {
                rhythmVisualDriver.Show(runner);
            }
        }

        private void HideRhythmSequence(RhythmSequenceRunner runner)
        {
            if (rhythmVisualDriver != null)
            {
                rhythmVisualDriver.Hide();
            }
        }

        private void ShowInputSetCollection(InputSetCollectionRunner runner)
        {
            if (inputSetVisualDriver != null)
            {
                inputSetVisualDriver.Show(runner);
            }
        }

        private void HideInputSetCollection(InputSetCollectionRunner runner)
        {
            if (inputSetVisualDriver != null)
            {
                inputSetVisualDriver.Hide();
            }
        }

        private void ShowShakeBalance(ShakeBalanceRunner runner)
        {
            if (shakeBalanceVisualDriver != null)
            {
                shakeBalanceVisualDriver.Show(runner);
            }
        }

        private void HideShakeBalance(ShakeBalanceRunner runner)
        {
            if (shakeBalanceVisualDriver != null)
            {
                shakeBalanceVisualDriver.Hide();
            }
        }
    }
}
