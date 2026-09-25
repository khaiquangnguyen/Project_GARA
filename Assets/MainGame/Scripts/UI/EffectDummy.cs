using UnityEngine;

// Lives on the root of the EffectExampleDummy prefab (and any future
// variant of it) — exposes which child GameObjects hold this dummy's
// per-character effect components (OnNotTargetedEffect, OnHitEffect) via
// direct Inspector-assigned references. CombatSceneManager references those
// two children directly as the templates it clones onto every character.
//
// Also doubles as an authoring-time preview tool: assign ANY character
// prefab to previewCharacterPrefab and use the Preview/Restore context menu
// entries (right-click the component header, or the "..." menu, works in
// both Edit and Play Mode) to see exactly what an effect looks like applied
// to that character — a temporary instance is spawned, the effect cloned
// onto it and triggered directly, with no combat/targeting system involved.
public class EffectDummy : MonoBehaviour
{
    [Tooltip("The child GameObject holding this dummy's OnNotTargetedEffect component — cloned onto every character at combat start.")]
    [SerializeField] private GameObject onNotTargetedEffect;

    [Tooltip("The child GameObject holding this dummy's OnHitEffect component — cloned onto every character at combat start.")]
    [SerializeField] private GameObject onHitEffect;

    [Header("Preview")]
    [Tooltip("Any character prefab (Dancer, FrozenTomato, a future one, ...) to preview effects against, as if it were the previewed character in combat.")]
    [SerializeField] private GameObject previewCharacterPrefab;

    public GameObject OnNotTargetedEffect => onNotTargetedEffect;
    public GameObject OnHitEffect => onHitEffect;

    private GameObject _previewInstance;
    private OnNotTargetedEffect _previewNotTargetedEffect;
    private OnHitEffect _previewHitEffect;

    [ContextMenu("Preview/Play Not-Targeted Effect")]
    public void Preview()
    {
        if (previewCharacterPrefab == null || onNotTargetedEffect == null)
        {
            Debug.LogWarning("EffectDummy.Preview: assign both previewCharacterPrefab and onNotTargetedEffect first.", this);
            return;
        }

        EnsurePreviewInstance();

        var effectClone = Instantiate(onNotTargetedEffect, _previewInstance.transform, false);
        _previewNotTargetedEffect = effectClone.GetComponent<OnNotTargetedEffect>();

        if (_previewNotTargetedEffect == null)
        {
            Debug.LogWarning("EffectDummy.Preview: onNotTargetedEffect has no OnNotTargetedEffect component.", this);
            return;
        }

        _previewNotTargetedEffect.OnTrigger();
    }

    [ContextMenu("Preview/Restore")]
    public void RestorePreview()
    {
        if (_previewNotTargetedEffect != null)
        {
            _previewNotTargetedEffect.OnDone();
        }
    }

    [ContextMenu("Preview/Play Hit Effect")]
    public void PreviewHit()
    {
        if (previewCharacterPrefab == null || onHitEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewHit: assign both previewCharacterPrefab and onHitEffect first.", this);
            return;
        }

        EnsurePreviewInstance();

        var effectClone = Instantiate(onHitEffect, _previewInstance.transform, false);
        _previewHitEffect = effectClone.GetComponent<OnHitEffect>();

        if (_previewHitEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewHit: onHitEffect has no OnHitEffect component.", this);
            return;
        }

        _previewHitEffect.OnTrigger();
    }

    [ContextMenu("Preview/Restore Hit Effect")]
    public void RestoreHitPreview()
    {
        if (_previewHitEffect != null)
        {
            _previewHitEffect.OnDone();
        }
    }

    private void EnsurePreviewInstance()
    {
        if (_previewInstance != null)
        {
            return;
        }

        _previewInstance = Instantiate(previewCharacterPrefab, transform.position, transform.rotation);
        _previewInstance.name = previewCharacterPrefab.name + " (Effect Preview)";
    }

    [ContextMenu("Preview/Clear")]
    public void ClearPreview()
    {
        if (_previewInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(_previewInstance);
            }
            else
            {
                DestroyImmediate(_previewInstance);
            }
        }

        _previewInstance = null;
        _previewNotTargetedEffect = null;
        _previewHitEffect = null;
    }

    private void OnDestroy()
    {
        ClearPreview();
    }
}
