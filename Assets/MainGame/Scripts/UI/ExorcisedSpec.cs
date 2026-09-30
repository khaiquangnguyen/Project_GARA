using UnityEngine;

// Tuning for an exorcised character's demon soul, read by OnExorcisedEffect.
[CreateAssetMenu(menuName = "GARA/Combat/Exorcised Spec", fileName = "ExorcisedSpec")]
public class ExorcisedSpec : ScriptableObject
{
    [Tooltip("Spawned over the character while exorcised; destroyed when it wears off.")]
    [SerializeField]
    private GameObject demonSoulPrefab;

    [Tooltip("From the character's root.")]
    [SerializeField]
    private Vector3 offset = new(0f, 1.5f, 0f);

    public GameObject DemonSoulPrefab => demonSoulPrefab;
    public Vector3 Offset => offset;
}
