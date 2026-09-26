using UnityEngine;

// Tuning for the current-actor ring, read by OnActiveActorEffect.
[CreateAssetMenu(menuName = "GARA/Combat/Active Actor Marker Spec", fileName = "ActiveActorMarkerSpec")]
public class ActiveActorMarkerSpec : ScriptableObject
{
    [Header("Colour")]
    [Tooltip("Ring tint while the actor is player-controlled.")]
    [SerializeField]
    private Color playerControlledColor = new(1f, 0.85f, 0.3f);

    [Tooltip("Ring tint while the actor is AI-controlled.")]
    [SerializeField]
    private Color aiControlledColor = new(1f, 0.35f, 0.35f);

    [Header("Ring")]
    [Tooltip("Ring scale at rest — squash Y to lay it flat on the ground.")]
    [SerializeField]
    private Vector2 ringScale = new(1.5f, 0.5f);

    [Tooltip("Extra scale at the peak of a pulse, as a fraction of ringScale.")]
    [Min(0f)]
    [SerializeField]
    private float ringPulseAmount = 0.08f;

    [Tooltip("Seconds for one full pulse.")]
    [Min(0.01f)]
    [SerializeField]
    private float ringPulsePeriod = 1.2f;

    public Color PlayerControlledColor => playerControlledColor;
    public Color AiControlledColor => aiControlledColor;
    public Vector2 RingScale => ringScale;
    public float RingPulseAmount => ringPulseAmount;
    public float RingPulsePeriod => ringPulsePeriod;
}
