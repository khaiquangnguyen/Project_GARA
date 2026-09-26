using GARA.Combat;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;

// Shows the target-selection arrow over its owner's head and a pulsing
// ring at its feet while the owner is the current actor
// (ActiveActorStateEvent).
// Cloned onto each character from the effect dummy, like OnHitEffect.
public class OnActiveActorEffect : MonoBehaviour, MMEventListener<ActiveActorStateEvent>
{
    [SerializeField] private ActiveActorMarkerSpec spec;

    [Tooltip("Spawned over the owner's head — the same TargetIndicator prefab TargetSelector uses.")]
    [SerializeField] private GameObject arrowPrefab;

    [Tooltip("Placed at the owner's feet (its root).")]
    [SerializeField] private SpriteRenderer ring;

    private GameObject _owner;
    private bool _shown;
    private float _shownAt;
    private GameObject _arrow;

    private void Awake()
    {
        _owner = transform.parent != null ? transform.parent.gameObject : null;
        SetRenderersEnabled(false);
    }

    private void OnEnable()
    {
        this.MMEventStartListening<ActiveActorStateEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<ActiveActorStateEvent>();
    }

    public void OnMMEvent(ActiveActorStateEvent activeActorStateEvent)
    {
        if (_owner == null || activeActorStateEvent.sceneRoot != _owner)
        {
            return;
        }

        if (activeActorStateEvent.isActive)
        {
            OnTrigger(activeActorStateEvent.isPlayerControlled);
        }
        else
        {
            OnDone();
        }
    }

    // Public so EffectDummy's preview can show it without combat running.
    public void OnTrigger(bool isPlayerControlled)
    {
        if (spec == null)
        {
            Debug.LogWarning($"{nameof(OnActiveActorEffect)} on {name} has no spec.", this);
            return;
        }

        if (_arrow == null && arrowPrefab != null)
        {
            _arrow = Instantiate(arrowPrefab, transform, false);
        }

        if (_arrow != null)
        {
            _arrow.transform.position = HeadTopOf(_owner);
        }

        if (ring != null)
        {
            ring.color = isPlayerControlled ? spec.PlayerControlledColor : spec.AiControlledColor;
            ring.transform.localPosition = Vector3.zero;
        }

        _shown = true;
        _shownAt = Time.time;
        SetRenderersEnabled(true);
        Animate();
    }

    public void OnDone()
    {
        _shown = false;
        SetRenderersEnabled(false);
    }

    private void Update()
    {
        if (_shown)
        {
            Animate();
        }
    }

    private void Animate()
    {
        var elapsed = Time.time - _shownAt;
        if (ring != null)
        {
            var pulse = 0.5f + 0.5f * Mathf.Sin(elapsed * 2f * Mathf.PI / spec.RingPulsePeriod);
            var scale = spec.RingScale * (1f + spec.RingPulseAmount * pulse);
            ring.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        }
    }

    // Top-centre of the owner's Spine mesh, or its root if it has none —
    // where TargetSelector puts its indicator.
    private static Vector3 HeadTopOf(GameObject owner)
    {
        var skeleton = owner != null ? owner.GetComponentInChildren<SkeletonRenderer>() : null;
        if (skeleton == null || !skeleton.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            return owner != null ? owner.transform.position : Vector3.zero;
        }

        var bounds = meshRenderer.bounds;
        return new Vector3(bounds.center.x, bounds.max.y, owner.transform.position.z);
    }

    private void SetRenderersEnabled(bool isEnabled)
    {
        // Re-enabling replays the indicator's own MMF_Player animation.
        if (_arrow != null)
        {
            _arrow.SetActive(isEnabled);
        }

        if (ring != null)
        {
            ring.enabled = isEnabled;
        }
    }
}
