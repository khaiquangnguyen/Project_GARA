using UnityEngine;
using UnityEngine.Rendering.Universal;

// Tuning for the Noir World's whole-screen Sin City look, read by
// NoirWorldScreenEffect.
[CreateAssetMenu(menuName = "GARA/Combat/Noir World Spec", fileName = "NoirWorldSpec")]
public class NoirWorldSpec : ScriptableObject
{
    [Tooltip("How far into the look the world ends up: 0 = full colour, 1 = full Sin City.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float amount = 1f;

    [Tooltip("Seconds for the look to fade in (without a flash).")]
    [Min(0f)]
    [SerializeField]
    private float fadeInDuration = 0.4f;

    [Tooltip("Seconds for colour to come back as the Noir World ends.")]
    [Min(0f)]
    [SerializeField]
    private float fadeOutDuration = 0.8f;

    [SerializeField]
    private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Seconds of negative (inverted) image on entering, then a hard cut into the look. 0 = fade in instead.")]
    [Min(0f)]
    [SerializeField]
    private float invertFlashDuration = 0.1f;

    [SerializeField]
    private SinCityLook look = new();

    [Header("Grain")]
    [Tooltip("URP Film Grain pattern, faded in with the look.")]
    [SerializeField]
    private FilmGrainLookup grainType = FilmGrainLookup.Medium3;

    [Range(0f, 1f)]
    [SerializeField]
    private float grainIntensity = 0.6f;

    [Tooltip("How much grain is removed in bright areas: 0 = even grain, 1 = grain mostly in the shadows.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float grainResponse = 0.6f;

    public float Amount => amount;
    public float FadeInDuration => fadeInDuration;
    public float FadeOutDuration => fadeOutDuration;
    public AnimationCurve FadeCurve => fadeCurve;
    public float InvertFlashDuration => invertFlashDuration;
    public SinCityLook Look => look;
    public FilmGrainLookup GrainType => grainType;
    public float GrainIntensity => grainIntensity;
    public float GrainResponse => grainResponse;
}
