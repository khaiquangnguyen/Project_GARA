using UnityEngine;
using GARA.Combat;

// Plain component — deliberately not part of the GARA.Combat assembly or
// namespace. It knows nothing about CombatParticipant/BattleContext/turn
// groups; it only listens for CombatPhaseController's static events (plain
// Sprite[]/bool/no-payload signals) and forwards them to the objects it
// owns. Each slot's own TurnAvatar (and each announcement's own
// GameObject) owns how it actually renders — this class just wires the
// turn-order system's events to the right objects, so any individual
// visual can change without this needing to.
public class CombatOverlayManager : MonoBehaviour
{
    [Tooltip("Exactly 6 slots, left to right — slot 0 is always the current turn, since the queue itself shifts left as turns are consumed.")]
    [SerializeField] private TurnAvatar[] turnOrderSlots = new TurnAvatar[6];

    [Tooltip("Shown while a player character is acting, hidden otherwise.")]
    [SerializeField] private GameObject playerTurnAnnouncement;

    [Tooltip("Shown while an enemy is acting, hidden otherwise.")]
    [SerializeField] private GameObject enemyTurnAnnouncement;

    [Tooltip("Toggled on the instant a special is used; toggled off again when the next turn starts.")]
    [SerializeField] private GameObject specialUsedAnnouncement;

    [Tooltip("Toggled on the instant a basic-attack sequence starts; toggled off again when the next turn starts.")]
    [SerializeField] private GameObject basicAttackStartedAnnouncement;

    [Tooltip("Toggled on the instant a combo finisher lands; toggled off again when the next turn starts.")]
    [SerializeField] private GameObject comboPerformedAnnouncement;

    [Tooltip("The single authored 'what happens to a non-targeted character' template — mirrors a character's own structure (a Visual child) plus its own OnNotTargetedEffect child, exposed via EffectDummy. That child is cloned onto every character once combat starts.")]
    [SerializeField] private EffectDummy effectDummyPrefab;

    private bool _hasClonedCharacterEffects;

    private void OnEnable()
    {
        CombatPhaseController.TurnOrderChanged += RefreshTurnOrder;
        CombatPhaseController.TurnFactionChanged += ToggleTurnAnnouncement;
        CombatPhaseController.SpecialUsedAnnouncement += ShowSpecialUsedAnnouncement;
        CombatPhaseController.BasicAttackStartedAnnouncement += ShowBasicAttackStartedAnnouncement;
        CombatPhaseController.ComboPerformedAnnouncement += ShowComboPerformedAnnouncement;
    }

    private void OnDisable()
    {
        CombatPhaseController.TurnOrderChanged -= RefreshTurnOrder;
        CombatPhaseController.TurnFactionChanged -= ToggleTurnAnnouncement;
        CombatPhaseController.SpecialUsedAnnouncement -= ShowSpecialUsedAnnouncement;
        CombatPhaseController.BasicAttackStartedAnnouncement -= ShowBasicAttackStartedAnnouncement;
        CombatPhaseController.ComboPerformedAnnouncement -= ShowComboPerformedAnnouncement;
    }

    public void RefreshTurnOrder(Sprite[] portraits)
    {
        // First time the turn order is populated, every character has
        // definitely finished spawning (CombatManager only raises this
        // once GenerateParticipants is fully done) — the reliable moment
        // to clone the per-character effects onto each of them, rather than
        // racing Unity's cross-object Start() ordering.
        if (!_hasClonedCharacterEffects)
        {
            CloneEffectOntoEveryCharacter(effectDummyPrefab != null ? effectDummyPrefab.OnNotTargetedEffect : null);
            CloneEffectOntoEveryCharacter(effectDummyPrefab != null ? effectDummyPrefab.OnHitEffect : null);
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

    private void CloneEffectOntoEveryCharacter(GameObject effectPrefab)
    {
        if (effectPrefab == null)
        {
            return;
        }

        foreach (var executor in FindObjectsByType<AttackExecutor>(FindObjectsSortMode.None))
        {
            Instantiate(effectPrefab, executor.transform, false);
        }
    }

    public void ToggleTurnAnnouncement(bool isPlayerTurn)
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

        if (basicAttackStartedAnnouncement != null)
        {
            basicAttackStartedAnnouncement.SetActive(false);
        }

        if (comboPerformedAnnouncement != null)
        {
            comboPerformedAnnouncement.SetActive(false);
        }
    }

    private void ShowSpecialUsedAnnouncement()
    {
        if (specialUsedAnnouncement != null)
        {
            specialUsedAnnouncement.SetActive(true);
        }
    }

    private void ShowBasicAttackStartedAnnouncement()
    {
        if (basicAttackStartedAnnouncement != null)
        {
            basicAttackStartedAnnouncement.SetActive(true);
        }
    }

    private void ShowComboPerformedAnnouncement()
    {
        if (comboPerformedAnnouncement != null)
        {
            comboPerformedAnnouncement.SetActive(true);
        }
    }
}
