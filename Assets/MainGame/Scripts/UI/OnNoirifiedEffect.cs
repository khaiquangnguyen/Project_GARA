using System.Collections.Generic;
using GARA.Characters;
using GARA.Combat;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;

// Turns its owner Sin City black and white once it's noirified
// (StatusAppliedStateEvent), by swapping the owner's atlas materials for
// GARA/Spine/Skeleton Sin City copies: a negative flash then a hard cut in
// (or a fade, without a flash), faded back out on restore. Also worn by
// everyone while the Noir World look is up, for the white rim.
// Cloned onto each character from the effect dummy, like OnHitEffect.
public class OnNoirifiedEffect : MonoBehaviour, MMEventListener<StatusAppliedStateEvent>
{
    private static readonly int GrayPhaseId = Shader.PropertyToID("_GrayPhase");
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

    [SerializeField] private NoirifiedSpec spec;

    private GameObject _owner;
    private SkeletonRenderer _skeletonRenderer;
    private readonly Dictionary<Material, Material> _grayByOriginal = new();
    private float _phase;
    private float _fromPhase;
    private float _toPhase;
    private float _fadeStartedAt;
    private bool _fading;
    private float _flashEndsAt = -1f;
    private bool _noirified;
    private bool _inNoirWorld;

    private void Awake()
    {
        _owner = transform.parent != null ? transform.parent.gameObject : null;
        _skeletonRenderer = _owner != null ? _owner.GetComponentInChildren<SkeletonRenderer>() : null;
    }

    private void OnEnable()
    {
        this.MMEventStartListening<StatusAppliedStateEvent>();
        NoirWorldScreenEffect.Shown += OnNoirWorldShown;
        NoirWorldScreenEffect.Hidden += OnNoirWorldHidden;
    }

    private void OnDisable()
    {
        this.MMEventStopListening<StatusAppliedStateEvent>();
        NoirWorldScreenEffect.Shown -= OnNoirWorldShown;
        NoirWorldScreenEffect.Hidden -= OnNoirWorldHidden;
    }

    private void OnDestroy()
    {
        RemoveOverrides();
    }

    public void OnMMEvent(StatusAppliedStateEvent statusEvent)
    {
        if (_owner == null || statusEvent.sceneRoot != _owner || statusEvent.kind != StatusEffectKind.Noirified)
        {
            return;
        }

        OnTrigger();
    }

    private void OnNoirWorldShown()
    {
        var wasShowing = _noirified || _inNoirWorld;
        _inNoirWorld = true;
        if (!wasShowing)
        {
            Show();
        }
    }

    private void OnNoirWorldHidden()
    {
        _inNoirWorld = false;
        if (!_noirified)
        {
            Hide();
        }
    }

    // Public so EffectDummy's preview can play it without combat running.
    public void OnTrigger()
    {
        var wasShowing = _noirified || _inNoirWorld;
        _noirified = true;
        if (!wasShowing)
        {
            Show();
        }
    }

    // Fades back to full colour (unless the Noir World still needs the look),
    // then puts the original materials back.
    public void OnDone()
    {
        _noirified = false;
        if (!_inNoirWorld)
        {
            Hide();
        }
    }

    private void Show()
    {
        if (spec == null || spec.Shader == null)
        {
            Debug.LogWarning($"{nameof(OnNoirifiedEffect)} on {name} needs a spec with a Sin City shader.", this);
            return;
        }

        if (!EnsureOverrides())
        {
            return;
        }

        // Edit-mode previews get no Update to end a flash, so they just cut.
        if (spec.InvertFlashDuration > 0f && Application.isPlaying)
        {
            _fading = false;
            _flashEndsAt = Time.time + spec.InvertFlashDuration;
            SetPhase(spec.Amount);
            return;
        }

        FadeTo(spec.Amount);
    }

    private void Hide()
    {
        if (_grayByOriginal.Count == 0)
        {
            return;
        }

        FadeTo(0f);
    }

    private void FadeTo(float phase)
    {
        _flashEndsAt = -1f;
        _fromPhase = _phase;
        _toPhase = phase;
        _fadeStartedAt = Time.time;
        _fading = true;

        // Edit-mode previews get no Update, so snap straight to the end.
        if (!Application.isPlaying || spec.FadeDuration <= 0f)
        {
            FinishFade();
        }
    }

    private void Update()
    {
        if (_flashEndsAt >= 0f && Time.time >= _flashEndsAt)
        {
            _flashEndsAt = -1f;
            SetPhase(_phase);
        }

        if (!_fading)
        {
            return;
        }

        var t = Mathf.Clamp01((Time.time - _fadeStartedAt) / spec.FadeDuration);
        if (t >= 1f)
        {
            FinishFade();
            return;
        }

        SetPhase(Mathf.LerpUnclamped(_fromPhase, _toPhase, spec.FadeCurve.Evaluate(t)));
    }

    private void FinishFade()
    {
        _fading = false;
        SetPhase(_toPhase);
        if (_toPhase <= 0f)
        {
            RemoveOverrides();
        }
    }

    // Re-applies the whole spec too, so tweaking it mid-play shows next change.
    private void SetPhase(float phase)
    {
        _phase = phase;
        var invert = _flashEndsAt >= 0f ? 1f : 0f;
        var rim = spec.RimColor * phase;
        foreach (var gray in _grayByOriginal.Values)
        {
            spec.Look.ApplyTo(gray, invert);
            gray.SetFloat(GrayPhaseId, phase);
            gray.SetColor(OutlineColorId, rim);
            gray.SetFloat(OutlineWidthId, spec.RimWidth);
        }
    }

    // One grayscale copy per atlas material, used by this character only.
    private bool EnsureOverrides()
    {
        if (_grayByOriginal.Count > 0)
        {
            return true;
        }

        var dataAsset = _skeletonRenderer != null ? _skeletonRenderer.SkeletonDataAsset : null;
        if (dataAsset == null)
        {
            return false;
        }

        foreach (var atlasAsset in dataAsset.atlasAssets)
        {
            if (atlasAsset == null)
            {
                continue;
            }

            foreach (var original in atlasAsset.Materials)
            {
                if (original == null || _grayByOriginal.ContainsKey(original))
                {
                    continue;
                }

                var gray = new Material(spec.Shader) { name = original.name + " (Noir)" };
                gray.CopyPropertiesFromMaterial(original);
                gray.shaderKeywords = original.shaderKeywords;
                _grayByOriginal[original] = gray;
                _skeletonRenderer.CustomMaterialOverride[original] = gray;
            }
        }

        SetPhase(_phase);
        return _grayByOriginal.Count > 0;
    }

    private void RemoveOverrides()
    {
        foreach (var (original, gray) in _grayByOriginal)
        {
            if (_skeletonRenderer != null)
            {
                _skeletonRenderer.CustomMaterialOverride.Remove(original);
            }

            if (Application.isPlaying)
            {
                Destroy(gray);
            }
            else
            {
                DestroyImmediate(gray);
            }
        }

        _grayByOriginal.Clear();
        _phase = 0f;
        _fading = false;
        _flashEndsAt = -1f;
    }
}
