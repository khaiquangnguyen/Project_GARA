using System;
using GARA.Characters;
using GARA.Combat;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Turns the whole screen Sin City while the Noir World is active, by driving
// the globals of the renderer's Noir World full-screen pass
// (GARA/Noir World Fullscreen). Lives once in the combat scene. Only the
// world camera gets the look; other cameras (the HUD's) see phase 0.
// Film grain comes from a post-processing Volume weighted by the same fade.
// Shown/Hidden let every character put on its own look (and white rim) so
// it doesn't melt into the world's ink.
public class NoirWorldScreenEffect : MonoBehaviour
{
    public static event Action Shown;
    public static event Action Hidden;

    private const string GlobalPrefix = "_NoirWorld";
    private static readonly int PhaseId = Shader.PropertyToID(GlobalPrefix + "Phase");

    [SerializeField] private NoirWorldSpec spec;

    [Tooltip("The camera that renders the battle. Others, like the HUD's, stay in colour.")]
    [SerializeField] private Camera worldCamera;

    [Tooltip("Global Volume for the grain; its profile is built from the spec at runtime. Keep its priority above other volumes.")]
    [SerializeField] private Volume grainVolume;

    private float _phase;
    private float _fromPhase;
    private float _toPhase;
    private float _fadeDuration;
    private float _fadeStartedAt;
    private bool _fading;
    private float _flashEndsAt = -1f;
    private VolumeProfile _grainProfile;
    private FilmGrain _grain;

    public bool IsShowing => _toPhase > 0f;

    // Runtime profile, so the spec never writes into a shared profile asset.
    private void Awake()
    {
        if (grainVolume == null)
        {
            return;
        }

        _grainProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        _grain = _grainProfile.Add<FilmGrain>(true);
        grainVolume.profile = _grainProfile;
        grainVolume.weight = 0f;
    }

    private void OnDestroy()
    {
        if (_grainProfile != null)
        {
            Destroy(_grainProfile);
        }
    }

    private void OnEnable()
    {
        CombatSceneManager.NoirWorldEntered += OnNoirWorldEntered;
        CombatSceneManager.NoirWorldExited += OnNoirWorldExited;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    // Globals outlive play mode, so never leave the screen stuck in noir.
    private void OnDisable()
    {
        CombatSceneManager.NoirWorldEntered -= OnNoirWorldEntered;
        CombatSceneManager.NoirWorldExited -= OnNoirWorldExited;
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        _fading = false;
        _flashEndsAt = -1f;
        _toPhase = 0f;
        _phase = 0f;
        Shader.SetGlobalFloat(PhaseId, 0f);
        if (grainVolume != null)
        {
            grainVolume.weight = 0f;
        }
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
    {
        Shader.SetGlobalFloat(PhaseId, renderingCamera == worldCamera ? _phase : 0f);
    }

    private void OnNoirWorldEntered(NoirWorld world)
    {
        Show();
    }

    private void OnNoirWorldExited(NoirWorld world)
    {
        Hide();
    }

    // Public so the Character Manager can preview it without a Noir World.
    public void Show()
    {
        if (spec == null)
        {
            Debug.LogWarning($"{nameof(NoirWorldScreenEffect)} on {name} needs a spec.", this);
            return;
        }

        if (spec.InvertFlashDuration > 0f)
        {
            _fading = false;
            _toPhase = spec.Amount;
            _flashEndsAt = Time.time + spec.InvertFlashDuration;
            SetPhase(spec.Amount);
            Shown?.Invoke();
            return;
        }

        FadeTo(spec.Amount, spec.FadeInDuration);
        Shown?.Invoke();
    }

    public void Hide()
    {
        if (spec == null)
        {
            return;
        }

        FadeTo(0f, spec.FadeOutDuration);
        Hidden?.Invoke();
    }

    private void FadeTo(float phase, float duration)
    {
        _flashEndsAt = -1f;
        _fromPhase = _phase;
        _toPhase = phase;
        _fadeDuration = duration;
        _fadeStartedAt = Time.time;
        _fading = true;

        if (duration <= 0f)
        {
            _fading = false;
            SetPhase(phase);
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

        var t = Mathf.Clamp01((Time.time - _fadeStartedAt) / _fadeDuration);
        SetPhase(Mathf.LerpUnclamped(_fromPhase, _toPhase, spec.FadeCurve.Evaluate(t)));
        if (t >= 1f)
        {
            _fading = false;
        }
    }

    private void SetPhase(float phase)
    {
        _phase = phase;
        spec.Look.ApplyGlobal(GlobalPrefix, _flashEndsAt >= 0f ? 1f : 0f);
        if (_grain != null)
        {
            _grain.type.value = spec.GrainType;
            _grain.intensity.value = spec.GrainIntensity;
            _grain.response.value = spec.GrainResponse;
            grainVolume.weight = phase;
        }
    }
}
