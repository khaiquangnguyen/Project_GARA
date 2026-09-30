using GARA.Characters;
using GARA.Combat;
using MoreMountains.Tools;
using UnityEngine;

// Reveals its owner's demon soul while exorcised (StatusAppliedStateEvent)
// and clears it when the status ends (StatusEndedStateEvent).
// Cloned onto each character from the effect dummy, like OnHitEffect.
public class OnExorcisedEffect : MonoBehaviour, MMEventListener<StatusAppliedStateEvent>, MMEventListener<StatusEndedStateEvent>
{
    [SerializeField] private ExorcisedSpec spec;

    private GameObject _owner;
    private GameObject _soul;

    private void Awake()
    {
        _owner = transform.parent != null ? transform.parent.gameObject : null;
    }

    private void OnEnable()
    {
        this.MMEventStartListening<StatusAppliedStateEvent>();
        this.MMEventStartListening<StatusEndedStateEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<StatusAppliedStateEvent>();
        this.MMEventStopListening<StatusEndedStateEvent>();
    }

    private void OnDestroy()
    {
        OnDone();
    }

    public void OnMMEvent(StatusAppliedStateEvent statusEvent)
    {
        if (_owner != null && statusEvent.sceneRoot == _owner && statusEvent.kind == StatusEffectKind.Exorcised)
        {
            OnTrigger();
        }
    }

    public void OnMMEvent(StatusEndedStateEvent statusEvent)
    {
        if (_owner != null && statusEvent.sceneRoot == _owner && statusEvent.kind == StatusEffectKind.Exorcised)
        {
            OnDone();
        }
    }

    // Public so EffectDummy's preview can play it without combat running.
    public void OnTrigger()
    {
        if (_soul != null)
        {
            return;
        }

        if (spec == null || spec.DemonSoulPrefab == null)
        {
            Debug.LogWarning($"{nameof(OnExorcisedEffect)} on {name} needs a spec with a demon soul prefab.", this);
            return;
        }

        var parent = _owner != null ? _owner.transform : transform;
        _soul = Instantiate(spec.DemonSoulPrefab, parent, false);
        _soul.transform.localPosition = spec.Offset;
    }

    public void OnDone()
    {
        if (_soul == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(_soul);
        }
        else
        {
            DestroyImmediate(_soul);
        }

        _soul = null;
    }
}
