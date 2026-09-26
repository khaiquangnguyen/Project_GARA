using UnityEngine;

// Tuning for the black-and-white fade, read by OnNoirifiedEffect.
[CreateAssetMenu(menuName = "GARA/Combat/Noirified Spec", fileName = "NoirifiedSpec")]
public class NoirifiedSpec : ScriptableObject
{
    [Tooltip("Spine/Special/Skeleton Grayscale — swapped in for the character's atlas materials.")]
    [SerializeField]
    private Shader grayscaleShader;

    [Tooltip("How grey the character ends up: 0 = full colour, 1 = fully grey.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float grayAmount = 1f;

    [Tooltip("Seconds for the colour to drain out (and back, when restored).")]
    [Min(0f)]
    [SerializeField]
    private float fadeDuration = 0.6f;

    [SerializeField]
    private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public Shader GrayscaleShader => grayscaleShader;
    public float GrayAmount => grayAmount;
    public float FadeDuration => fadeDuration;
    public AnimationCurve FadeCurve => fadeCurve;
}
