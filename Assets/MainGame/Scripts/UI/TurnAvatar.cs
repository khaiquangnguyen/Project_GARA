using UnityEngine;

// One turn-order slot's own visual. Owns exactly how a portrait is shown
// and how "this is the current turn" vs. "passed/upcoming" is represented —
// today that's just alpha (50%/100%) on a SpriteRenderer, but every effect
// lives behind this component's API so it can be swapped out (a pulse, an
// outline, a scale-up, ...) without CombatOverlayManager or anything else
// that drives the turn order needing to change.
[RequireComponent(typeof(SpriteRenderer))]
public class TurnAvatar : MonoBehaviour
{
    private const float InactiveAlpha = 0.5f;
    private const float CurrentTurnAlpha = 1f;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void SetPortrait(Sprite portrait)
    {
        spriteRenderer.enabled = portrait != null;
        spriteRenderer.sprite = portrait;
    }

    public void SetCurrent(bool isCurrent)
    {
        SetAlpha(isCurrent ? CurrentTurnAlpha : InactiveAlpha);
    }

    private void SetAlpha(float alpha)
    {
        var color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
