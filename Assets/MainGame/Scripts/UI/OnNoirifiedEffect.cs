using System.Collections.Generic;
using GARA.Characters;
using GARA.Combat;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;

// Drains its owner to black and white once it's noirified
// (StatusAppliedStateEvent), by swapping the owner's atlas materials for
// grayscale copies and fading their _GrayPhase.
// Cloned onto each character from the effect dummy, like OnHitEffect.
public class OnNoirifiedEffect : MonoBehaviour, MMEventListener<StatusAppliedStateEvent>
{
    private static readonly int GrayPhaseId = Shader.PropertyToID("_GrayPhase");

    [SerializeField] private NoirifiedSpec spec;

    private GameObject _owner;
    private SkeletonRenderer _skeletonRenderer;
    private readonly Dictionary<Material, Material> _grayByOriginal = new();
    private float _phase;
    private float _fromPhase;
    private float _toPhase;
    private float _fadeStartedAt;
    private bool _fading;

    private void Awake()
    {
        _owner = transform.parent != null ? transform.parent.gameObject : null;
        _skeletonRenderer = _owner != null ? _owner.GetComponentInChildren<SkeletonRenderer>() : null;
    }

    private void OnEnable()
    {
        this.MMEventStartListening<StatusAppliedStateEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<StatusAppliedStateEvent>();
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

    // Public so EffectDummy's preview can play it without combat running.
    public void OnTrigger()
    {
        if (spec == null || spec.GrayscaleShader == null)
        {
            Debug.LogWarning($"{nameof(OnNoirifiedEffect)} on {name} needs a spec with a grayscale shader.", this);
            return;
        }

        if (!EnsureOverrides())
        {
            return;
        }

        FadeTo(spec.GrayAmount);
    }

    // Fades back to full colour, then puts the original materials back.
    public void OnDone()
    {
        if (_grayByOriginal.Count == 0)
        {
            return;
        }

        FadeTo(0f);
    }

    private void FadeTo(float phase)
    {
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

    private void SetPhase(float phase)
    {
        _phase = phase;
        foreach (var gray in _grayByOriginal.Values)
        {
            gray.SetFloat(GrayPhaseId, phase);
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

                var gray = new Material(spec.GrayscaleShader) { name = original.name + " (Noir)" };
                gray.CopyPropertiesFromMaterial(original);
                gray.shaderKeywords = original.shaderKeywords;
                gray.SetFloat(GrayPhaseId, _phase);
                _grayByOriginal[original] = gray;
                _skeletonRenderer.CustomMaterialOverride[original] = gray;
            }
        }

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
    }
}
