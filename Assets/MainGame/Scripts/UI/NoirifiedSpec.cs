using UnityEngine;
using UnityEngine.Serialization;

// Tuning for a noirified character's Sin City look, read by OnNoirifiedEffect.
[CreateAssetMenu(menuName = "GARA/Combat/Noirified Spec", fileName = "NoirifiedSpec")]
public class NoirifiedSpec : ScriptableObject
{
    [Tooltip("GARA/Spine/Skeleton Sin City — swapped in for the character's atlas materials.")]
    [FormerlySerializedAs("grayscaleShader")]
    [SerializeField]
    private Shader shader;

    [Tooltip("How far into the look the character ends up: 0 = full colour, 1 = full Sin City.")]
    [FormerlySerializedAs("grayAmount")]
    [Range(0f, 1f)]
    [SerializeField]
    private float amount = 1f;

    [Tooltip("Seconds for the look to fade in (without a flash) and back out when restored.")]
    [Min(0f)]
    [SerializeField]
    private float fadeDuration = 0.6f;

    [SerializeField]
    private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Seconds of negative (inverted) image on noirify, then a hard cut into the look. 0 = fade in instead.")]
    [Min(0f)]
    [SerializeField]
    private float invertFlashDuration = 0.08f;

    [SerializeField]
    private SinCityLook look = new();

    [Header("Rim")]
    [Tooltip("White edge that separates the black silhouette from a dark background. Alpha 0 = no rim.")]
    [ColorUsage(true, true)]
    [SerializeField]
    private Color rimColor = Color.white;

    [Range(0f, 8f)]
    [SerializeField]
    private float rimWidth = 3f;

    public Shader Shader => shader;
    public float Amount => amount;
    public float FadeDuration => fadeDuration;
    public AnimationCurve FadeCurve => fadeCurve;
    public float InvertFlashDuration => invertFlashDuration;
    public SinCityLook Look => look;
    public Color RimColor => rimColor;
    public float RimWidth => rimWidth;
}
