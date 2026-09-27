using System.Collections.Generic;
using DistantLands.Lumen;
using UnityEngine;

namespace GARA.Combat
{
    // Root of a card's spotlights: dims the stage while Lumen beams shine
    // down from above, sweep across onto the targets, sway on them, flare
    // on Hit(), then fade out and destroy themselves.
    public class EncoreSpotlightEffect : MonoBehaviour
    {
        // Seconds it waits for Hit() before fading out anyway, in case the
        // card never reports one.
        private const float HitTimeout = 30f;

        // Units from a beam's lit end back to its apex in the profile's
        // meshes (Lumen's 2D Spot Rays); stretched so the apex meets the light.
        private const float ProfileBeamLength = 15f;

        private const string BeamLayerName = "LumenFX";

        [Tooltip("Lumen profile for one beam. Its rays end at the player and run back along -Z to the light.")]
        [SerializeField]
        private LumenEffectProfile beamProfile;

        [Tooltip("One beam each; beams share targets round-robin.")]
        [SerializeField]
        private EncoreBeam[] beams =
        {
            new EncoreBeam { color = new Color(1f, 0.85f, 0.35f), sourceOffset = -6f, sweepFrom = 4f, swayPhase = 0f },
            new EncoreBeam { color = new Color(1f, 0.45f, 0.8f), sourceOffset = 0f, sweepFrom = -4.5f, swayPhase = 2.1f },
            new EncoreBeam { color = new Color(0.4f, 0.85f, 1f), sourceOffset = 6f, sweepFrom = -3f, swayPhase = 4.2f }
        };

        [Tooltip("World units above its target a beam's light hangs.")]
        [Min(0f)]
        [SerializeField]
        private float sourceHeight = 12f;

        [Tooltip("Seconds the beams take to sweep onto their targets.")]
        [Min(0f)]
        [SerializeField]
        private float sweepDuration = 0.5f;

        [Tooltip("World units a beam sways either side of its target once on it.")]
        [Min(0f)]
        [SerializeField]
        private float swayWidth = 0.35f;

        [Tooltip("Radians per second of the sway.")]
        [SerializeField]
        private float swaySpeed = 7f;

        [Min(0f)]
        [SerializeField]
        private float fadeInDuration = 0.15f;

        [Tooltip("Brightness the beams flare to on the finale's hit, settling back to 1.")]
        [Min(1f)]
        [SerializeField]
        private float hitFlare = 2.5f;

        [Min(0f)]
        [SerializeField]
        private float hitFlareDuration = 0.3f;

        [Tooltip("Optional. Prefab spawned at each target's body centre on the finale's hit — e.g. confetti.")]
        [SerializeField]
        private GameObject hitBurst;

        [Tooltip("Colour laid over the stage behind the characters; alpha is how dark. Clear for no dimming.")]
        [SerializeField]
        private Color dimColor = new Color(0f, 0f, 0f, 0.6f);

        [SerializeField]
        private string dimSortingLayer = "Character";

        [Tooltip("Keep below the characters so only the stage behind them darkens.")]
        [SerializeField]
        private int dimSortingOrder = -1000;

        [Tooltip("Seconds it stays after the finale's hit before fading out.")]
        [Min(0f)]
        [SerializeField]
        private float holdAfterHit = 0.8f;

        [Min(0f)]
        [SerializeField]
        private float fadeOutDuration = 0.35f;

        private readonly List<Transform> _anchors = new List<Transform>();
        private readonly List<Vector3> _aimOffsets = new List<Vector3>();
        private readonly List<LumenEffectPlayer> _players = new List<LumenEffectPlayer>();
        private SpriteRenderer _dim;
        private Sprite _dimSprite;
        private float _elapsed;
        private float _sinceHit;
        private bool _hasHit;

        // Aims the beams at each target's anchor + aim offset (its body
        // centre), following the anchors as they move.
        public void Show(IReadOnlyList<Transform> anchors, IReadOnlyList<Vector3> aimOffsets)
        {
            _anchors.Clear();
            _aimOffsets.Clear();
            for (var i = 0; i < anchors.Count; i++)
            {
                _anchors.Add(anchors[i]);
                _aimOffsets.Add(aimOffsets[i]);
            }

            _elapsed = 0f;
            _sinceHit = 0f;
            _hasHit = false;
            if (beamProfile == null || _anchors.Count == 0)
            {
                Destroy(gameObject);
                return;
            }

            SpawnDim();
            SpawnBeams();
            LateUpdate();
        }

        public void Hit()
        {
            if (_hasHit)
            {
                return;
            }

            _hasHit = true;
            if (hitBurst == null)
            {
                return;
            }

            for (var i = 0; i < _anchors.Count; i++)
            {
                if (_anchors[i] != null)
                {
                    Instantiate(hitBurst, _anchors[i].position + _aimOffsets[i], Quaternion.identity);
                }
            }
        }

        private void SpawnBeams()
        {
            var layer = LayerMask.NameToLayer(BeamLayerName);
            if (layer < 0)
            {
                Debug.LogWarning($"[{nameof(EncoreSpotlightEffect)}] No '{BeamLayerName}' layer — the renderer's Lumen feature draws only that layer.", this);
            }

            foreach (var beam in beams)
            {
                // Inactive until configured: the player redraws on enable.
                var beamObject = new GameObject("Encore Beam");
                beamObject.SetActive(false);
                beamObject.transform.SetParent(transform, false);
                if (layer >= 0)
                {
                    beamObject.layer = layer;
                }

                var player = beamObject.AddComponent<LumenEffectPlayer>();
                player.profile = beamProfile;
                player.color = beam.color;
                player.brightness = 0f;
                // Redrawn here each frame rather than trusting Lumen's manager
                // to pick the player up.
                player.updateFrequency = LumenEffectPlayer.UpdateFrequency.ViaScripting;
                beamObject.SetActive(true);
                _players.Add(player);
            }
        }

        private void LateUpdate()
        {
            if (_players.Count == 0)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (_hasHit)
            {
                _sinceHit += Time.deltaTime;
            }
            else if (_elapsed >= HitTimeout)
            {
                Hit();
            }

            var fade = fadeInDuration > 0f ? Mathf.Clamp01(_elapsed / fadeInDuration) : 1f;
            var flare = 1f;
            if (_hasHit)
            {
                if (hitFlareDuration > 0f && _sinceHit < hitFlareDuration)
                {
                    var settle = 1f - _sinceHit / hitFlareDuration;
                    flare = Mathf.Lerp(1f, hitFlare, settle * settle);
                }

                var fadingOut = _sinceHit - holdAfterHit;
                if (fadingOut > 0f)
                {
                    if (fadingOut >= fadeOutDuration)
                    {
                        Destroy(gameObject);
                        return;
                    }

                    fade *= 1f - fadingOut / fadeOutDuration;
                }
            }

            UpdateDim(fade);
            var brightness = fade * flare;

            var sweep = sweepDuration > 0f ? EaseOutCubic(Mathf.Clamp01(_elapsed / sweepDuration)) : 1f;
            for (var i = 0; i < _players.Count; i++)
            {
                var targetIndex = i % _anchors.Count;
                if (_anchors[targetIndex] == null)
                {
                    _players[i].brightness = 0f;
                    _players[i].RedoEffect(false);
                    continue;
                }

                var beam = beams[i];
                var target = _anchors[targetIndex].position + _aimOffsets[targetIndex];
                var sway = Mathf.Sin(_elapsed * swaySpeed + beam.swayPhase) * swayWidth * sweep;
                var aim = target + Vector3.right * (Mathf.LerpUnclamped(beam.sweepFrom, 0f, sweep) + sway);
                var source = target + new Vector3(beam.sourceOffset, sourceHeight, 0f);
                Aim(_players[i].transform, source, aim);
                _players[i].brightness = brightness;
                _players[i].RedoEffect(false);
            }
        }

        private void SpawnDim()
        {
            if (dimColor.a <= 0f)
            {
                return;
            }

            var texture = Texture2D.whiteTexture;
            _dimSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
            var dimObject = new GameObject("Stage Dim");
            dimObject.transform.SetParent(transform, false);
            _dim = dimObject.AddComponent<SpriteRenderer>();
            _dim.sprite = _dimSprite;
            _dim.sortingLayerName = dimSortingLayer;
            _dim.sortingOrder = dimSortingOrder;
        }

        // Covers the camera's view, darkened by fade.
        private void UpdateDim(float fade)
        {
            if (_dim == null)
            {
                return;
            }

            var color = dimColor;
            color.a *= fade;
            _dim.color = color;

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var halfHeight = camera.orthographicSize;
            var cameraPosition = camera.transform.position;
            _dim.transform.SetPositionAndRotation(new Vector3(cameraPosition.x, cameraPosition.y, 0f), Quaternion.identity);
            _dim.transform.localScale = new Vector3(halfHeight * camera.aspect * 2.2f, halfHeight * 2.2f, 1f);
        }

        // Lays the beam flat to the camera, lit end on aim, apex on source.
        private static void Aim(Transform beam, Vector3 source, Vector3 aim)
        {
            var towardsAim = aim - source;
            var length = towardsAim.magnitude;
            if (length < 0.001f)
            {
                return;
            }

            var direction = towardsAim / length;
            beam.SetPositionAndRotation(aim, Quaternion.LookRotation(direction, Vector3.Cross(direction, Vector3.forward)));
            beam.localScale = new Vector3(1f, 1f, length / ProfileBeamLength);
        }

        private void OnDestroy()
        {
            if (_dimSprite != null)
            {
                Destroy(_dimSprite);
            }
        }

        private static float EaseOutCubic(float t)
        {
            var u = 1f - t;
            return 1f - u * u * u;
        }
    }
}
