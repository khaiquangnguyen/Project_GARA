using GARA.InputSets;
using GARA.Rhythm;
using UnityEngine;

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

        [Tooltip("Shown while a player character is acting, hidden otherwise.")]
        [SerializeField] private GameObject playerTurnAnnouncement;

        [Tooltip("Shown while an enemy is acting, hidden otherwise.")]
        [SerializeField] private GameObject enemyTurnAnnouncement;

        [Tooltip("Toggled on the instant a special is used; toggled off again when the next turn starts.")]
        [SerializeField] private GameObject specialUsedAnnouncement;

        [Tooltip("The EffectDummy's OnNotTargetedEffect child — cloned onto every character once combat starts.")]
        [SerializeField] private GameObject notTargetedEffectTemplate;

        [Tooltip("The EffectDummy's OnHitEffect child — cloned onto every character once combat starts.")]
        [SerializeField] private GameObject hitEffectTemplate;

        [Tooltip("Scene instance of the RhythmVisualDriver prefab — shown whenever any rhythm sequence starts.")]
        [SerializeField] private RhythmVisualDriver rhythmVisualDriver;

        [Tooltip("Scene instance of the InputSetVisualDriver prefab — shown whenever any input set collection starts.")]
        [SerializeField] private InputSetVisualDriver inputSetVisualDriver;

        private bool _hasClonedCharacterEffects;

        private void EnableOverlay()
        {
            RhythmSequencePlayer.AnySequenceStarted += ShowRhythmSequence;
            RhythmSequencePlayer.AnySequenceEnded += HideRhythmSequence;
            InputSetCollectionPlayer.AnySetCollectionStarted += ShowInputSetCollection;
            InputSetCollectionPlayer.AnySetCollectionEnded += HideInputSetCollection;
        }

        private void DisableOverlay()
        {
            RhythmSequencePlayer.AnySequenceStarted -= ShowRhythmSequence;
            RhythmSequencePlayer.AnySequenceEnded -= HideRhythmSequence;
            InputSetCollectionPlayer.AnySetCollectionStarted -= ShowInputSetCollection;
            InputSetCollectionPlayer.AnySetCollectionEnded -= HideInputSetCollection;
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
                CloneEffectOntoEveryCharacter(notTargetedEffectTemplate);
                CloneEffectOntoEveryCharacter(hitEffectTemplate);
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

        private void CloneEffectOntoEveryCharacter(GameObject effectTemplate)
        {
            if (effectTemplate == null)
            {
                return;
            }

            foreach (var executor in _executors.Values)
            {
                Instantiate(effectTemplate, executor.transform, false);
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

            // A new turn starting clears any leftover action announcement from
            // whoever acted before.
            if (specialUsedAnnouncement != null)
            {
                specialUsedAnnouncement.SetActive(false);
            }
        }

        private void ShowSpecialUsedAnnouncement()
        {
            if (specialUsedAnnouncement != null)
            {
                specialUsedAnnouncement.SetActive(true);
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
    }
}
