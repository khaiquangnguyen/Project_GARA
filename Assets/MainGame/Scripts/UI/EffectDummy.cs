using System.Collections.Generic;
using UnityEngine;

// Lives on the root of each side's effect dummy prefab (PlayerEffectDummy,
// EnemyEffectDummy) — exposes which child GameObjects hold this dummy's
// per-character effect components (OnNotTargetedEffect — fade and shrink —
// OnHitEffect, OnParrySuccessEffect, OnJumpSuccessEffect, OnActiveActorEffect and OnNoirifiedEffect) via direct Inspector-assigned references.
// CombatSceneManager references those children directly as the templates
// it clones onto every character of that dummy's side.
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

    [Tooltip("The child GameObject holding this dummy's shrinking OnNotTargetedEffect component — cloned onto every character at combat start when the scene shrinks untargeted characters.")]
    [SerializeField] private GameObject onNotTargetedShrinkEffect;

    [Tooltip("The child GameObject holding this dummy's OnHitEffect component — cloned onto every character at combat start.")]
    [SerializeField] private GameObject onHitEffect;

    [Tooltip("The child GameObject holding this dummy's OnParrySuccessEffect component — cloned onto every character at combat start.")]
    [SerializeField] private GameObject onParrySuccessEffect;

    [Tooltip("The child GameObject holding this dummy's OnJumpSuccessEffect component — cloned onto every character at combat start.")]
    [SerializeField] private GameObject onJumpSuccessEffect;

    [Tooltip("The child GameObject holding this dummy's OnActiveActorEffect component — cloned onto every character at combat start.")]
    [SerializeField] private GameObject onActiveActorEffect;

    [Tooltip("The child GameObject holding this dummy's OnNoirifiedEffect component — cloned onto every character at combat start.")]
    [SerializeField] private GameObject onNoirifiedEffect;

    [Header("Preview")]
    [Tooltip("Any character prefab (Dancer, FrozenTomato, a future one, ...) to preview effects against, as if it were the previewed character in combat.")]
    [SerializeField] private GameObject previewCharacterPrefab;

    public GameObject OnNotTargetedEffect => onNotTargetedEffect;
    public GameObject OnNotTargetedShrinkEffect => onNotTargetedShrinkEffect;
    public GameObject OnHitEffect => onHitEffect;
    public GameObject OnParrySuccessEffect => onParrySuccessEffect;
    public GameObject OnJumpSuccessEffect => onJumpSuccessEffect;
    public GameObject OnActiveActorEffect => onActiveActorEffect;
    public GameObject OnNoirifiedEffect => onNoirifiedEffect;

    private GameObject _previewInstance;
    private readonly List<OnNotTargetedEffect> _previewNotTargetedEffects = new();
    private OnHitEffect _previewHitEffect;
    private OnParrySuccessEffect _previewParrySuccessEffect;
    private OnJumpSuccessEffect _previewJumpSuccessEffect;
    private OnActiveActorEffect _previewActiveActorEffect;
    private OnNoirifiedEffect _previewNoirifiedEffect;

    [ContextMenu("Preview/Play Not-Targeted Effect")]
    public void Preview()
    {
        PreviewNotTargeted(onNotTargetedEffect, nameof(onNotTargetedEffect));
    }

    [ContextMenu("Preview/Play Not-Targeted Shrink Effect")]
    public void PreviewShrink()
    {
        PreviewNotTargeted(onNotTargetedShrinkEffect, nameof(onNotTargetedShrinkEffect));
    }

    // Restores every not-targeted effect previewed so far (fade and shrink).
    [ContextMenu("Preview/Restore")]
    public void RestorePreview()
    {
        foreach (var effect in _previewNotTargetedEffects)
        {
            if (effect != null)
            {
                effect.OnDone();
            }
        }
    }

    private void PreviewNotTargeted(GameObject effectTemplate, string fieldName)
    {
        if (previewCharacterPrefab == null || effectTemplate == null)
        {
            Debug.LogWarning($"EffectDummy.Preview: assign both previewCharacterPrefab and {fieldName} first.", this);
            return;
        }

        EnsurePreviewInstance();

        var effectClone = Instantiate(effectTemplate, _previewInstance.transform, false);
        var effect = effectClone.GetComponent<OnNotTargetedEffect>();

        if (effect == null)
        {
            Debug.LogWarning($"EffectDummy.Preview: {fieldName} has no OnNotTargetedEffect component.", this);
            return;
        }

        _previewNotTargetedEffects.Add(effect);
        effect.OnTrigger();
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

    [ContextMenu("Preview/Play Parry Success Effect")]
    public void PreviewParrySuccess()
    {
        if (previewCharacterPrefab == null || onParrySuccessEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewParrySuccess: assign both previewCharacterPrefab and onParrySuccessEffect first.", this);
            return;
        }

        EnsurePreviewInstance();

        var effectClone = Instantiate(onParrySuccessEffect, _previewInstance.transform, false);
        _previewParrySuccessEffect = effectClone.GetComponent<OnParrySuccessEffect>();

        if (_previewParrySuccessEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewParrySuccess: onParrySuccessEffect has no OnParrySuccessEffect component.", this);
            return;
        }

        _previewParrySuccessEffect.OnTrigger();
    }

    [ContextMenu("Preview/Restore Parry Success Effect")]
    public void RestoreParrySuccessPreview()
    {
        if (_previewParrySuccessEffect != null)
        {
            _previewParrySuccessEffect.OnDone();
        }
    }

    [ContextMenu("Preview/Play Jump Success Effect")]
    public void PreviewJumpSuccess()
    {
        if (previewCharacterPrefab == null || onJumpSuccessEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewJumpSuccess: assign both previewCharacterPrefab and onJumpSuccessEffect first.", this);
            return;
        }

        EnsurePreviewInstance();

        var effectClone = Instantiate(onJumpSuccessEffect, _previewInstance.transform, false);
        _previewJumpSuccessEffect = effectClone.GetComponent<OnJumpSuccessEffect>();

        if (_previewJumpSuccessEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewJumpSuccess: onJumpSuccessEffect has no OnJumpSuccessEffect component.", this);
            return;
        }

        _previewJumpSuccessEffect.OnTrigger();
    }

    [ContextMenu("Preview/Restore Jump Success Effect")]
    public void RestoreJumpSuccessPreview()
    {
        if (_previewJumpSuccessEffect != null)
        {
            _previewJumpSuccessEffect.OnDone();
        }
    }

    [ContextMenu("Preview/Show Active Actor Marker (Player-Controlled)")]
    public void PreviewActiveActorPlayerControlled()
    {
        PreviewActiveActor(true);
    }

    [ContextMenu("Preview/Show Active Actor Marker (AI-Controlled)")]
    public void PreviewActiveActorAiControlled()
    {
        PreviewActiveActor(false);
    }

    private void PreviewActiveActor(bool isPlayerControlled)
    {
        if (previewCharacterPrefab == null || onActiveActorEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewActiveActor: assign both previewCharacterPrefab and onActiveActorEffect first.", this);
            return;
        }

        EnsurePreviewInstance();

        if (_previewActiveActorEffect == null)
        {
            var effectClone = Instantiate(onActiveActorEffect, _previewInstance.transform, false);
            _previewActiveActorEffect = effectClone.GetComponent<OnActiveActorEffect>();
        }

        if (_previewActiveActorEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewActiveActor: onActiveActorEffect has no OnActiveActorEffect component.", this);
            return;
        }

        _previewActiveActorEffect.OnTrigger(isPlayerControlled);
    }

    [ContextMenu("Preview/Hide Active Actor Marker")]
    public void RestoreActiveActorPreview()
    {
        if (_previewActiveActorEffect != null)
        {
            _previewActiveActorEffect.OnDone();
        }
    }

    [ContextMenu("Preview/Play Noirified Effect")]
    public void PreviewNoirified()
    {
        if (previewCharacterPrefab == null || onNoirifiedEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewNoirified: assign both previewCharacterPrefab and onNoirifiedEffect first.", this);
            return;
        }

        EnsurePreviewInstance();

        if (_previewNoirifiedEffect == null)
        {
            var effectClone = Instantiate(onNoirifiedEffect, _previewInstance.transform, false);
            _previewNoirifiedEffect = effectClone.GetComponent<OnNoirifiedEffect>();
        }

        if (_previewNoirifiedEffect == null)
        {
            Debug.LogWarning("EffectDummy.PreviewNoirified: onNoirifiedEffect has no OnNoirifiedEffect component.", this);
            return;
        }

        _previewNoirifiedEffect.OnTrigger();
    }

    [ContextMenu("Preview/Restore Noirified Effect")]
    public void RestoreNoirifiedPreview()
    {
        if (_previewNoirifiedEffect != null)
        {
            _previewNoirifiedEffect.OnDone();
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
        _previewNotTargetedEffects.Clear();
        _previewHitEffect = null;
        _previewParrySuccessEffect = null;
        _previewJumpSuccessEffect = null;
        _previewActiveActorEffect = null;
        _previewNoirifiedEffect = null;
    }

    private void OnDestroy()
    {
        ClearPreview();
    }
}
