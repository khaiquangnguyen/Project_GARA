using System.Collections.Generic;
using GARA.InputSets;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Characters.Gambler
{
    // Shows any QtePlayer's run: one ring per open prompt (timing arc and
    // sweeping needle), stacked with the oldest, the one a press resolves,
    // in front. The token to press sits in the front ring. Lives in the scene.
    public class QteView : MonoBehaviour
    {
        private const int TextureSize = 256;
        private const int MaxStackDepth = 8;

        [Tooltip("Token icons and colors.")]
        [SerializeField]
        [Expandable]
        private InputSetVisualSpec spec;

        [Tooltip("Optional. Input set token shown in the front ring.")]
        [FormerlySerializedAs("arrowPrefab")]
        [SerializeField]
        private InputSetTokenView tokenPrefab;

        [Tooltip("World-space spot the front ring centers on.")]
        [FormerlySerializedAs("arrowAnchor")]
        [SerializeField]
        private Transform anchor;

        [Header("Ring")]
        [Tooltip("Ring diameter in world units.")]
        [FormerlySerializedAs("ringStartDiameter")]
        [Min(0.1f)]
        [SerializeField]
        private float ringDiameter = 3.2f;

        [Tooltip("Ring line width as a fraction of its diameter.")]
        [Range(0.01f, 0.5f)]
        [SerializeField]
        private float ringThickness = 0.04f;

        [Tooltip("Arc line width as a fraction of the ring's diameter.")]
        [Range(0.01f, 0.5f)]
        [SerializeField]
        private float arcThickness = 0.1f;

        [Tooltip("Needle width in world units.")]
        [Min(0.01f)]
        [SerializeField]
        private float needleWidth = 0.08f;

        [SerializeField]
        private Color ringColor = new Color(1f, 1f, 1f, 0.45f);

        [SerializeField]
        private Color arcColor = new Color(1f, 0.85f, 0.3f, 1f);

        [SerializeField]
        private Color needleColor = Color.white;

        [SerializeField]
        private Color hitColor = new Color(0.45f, 1f, 0.55f, 1f);

        [SerializeField]
        private Color missColor = new Color(1f, 0.35f, 0.35f, 1f);

        [Tooltip("Ring scale kick on a hit.")]
        [Min(1f)]
        [SerializeField]
        private float hitPunchScale = 1.15f;

        [Min(0.01f)]
        [SerializeField]
        private float punchSeconds = 0.15f;

        [Tooltip("Seconds a resolved ring fades out over.")]
        [Min(0.01f)]
        [SerializeField]
        private float fadeSeconds = 0.25f;

        [FormerlySerializedAs("ringSortingOrder")]
        [SerializeField]
        private int sortingOrder = 36;

        [Header("Stack")]
        [Tooltip("Offset of each ring behind the front one, in world units.")]
        [SerializeField]
        private Vector2 stackOffset = new Vector2(1.1f, 0.7f);

        [Tooltip("Scale of each ring relative to the one in front.")]
        [Range(0.3f, 1f)]
        [SerializeField]
        private float stackScale = 0.75f;

        [Tooltip("Opacity of each ring relative to the one in front.")]
        [Range(0.1f, 1f)]
        [SerializeField]
        private float stackFade = 0.6f;

        [Tooltip("How fast rings slide forward as the front one resolves.")]
        [Min(0.1f)]
        [SerializeField]
        private float slideSpeed = 14f;

        [Header("Count")]
        [Tooltip("Optional. Shows the prompt count; unset spawns a plain label.")]
        [SerializeField]
        private TMP_Text countText;

        [Tooltip("Spawned label only: offset from the anchor, in world units.")]
        [SerializeField]
        private Vector2 countOffset = new Vector2(2.4f, 0f);

        [Tooltip("Spawned label only.")]
        [Min(0.1f)]
        [SerializeField]
        private float countFontSize = 6f;

        private static readonly Dictionary<int, Sprite> RingSprites = new Dictionary<int, Sprite>();
        private static Sprite _needleSprite;

        private readonly List<PromptVisual> _shown = new List<PromptVisual>();
        private readonly Stack<PromptVisual> _pool = new Stack<PromptVisual>();

        private QtePlayer _player;
        private QteRunner _runner;
        private Transform _container;
        private InputSetTokenView _token;
        private QtePrompt _tokenPrompt;

        private void OnEnable()
        {
            QtePlayer.AnyStarted += Show;
            QtePlayer.AnyEnded += Hide;
            SetVisible(false);
        }

        private void OnDisable()
        {
            QtePlayer.AnyStarted -= Show;
            QtePlayer.AnyEnded -= Hide;
            Unbind();
            _player = null;
        }

        private void Update()
        {
            if (_runner == null || _container == null)
            {
                return;
            }

            _container.position = anchor.position;
            var blend = 1f - Mathf.Exp(-slideSpeed * Time.deltaTime);
            for (var i = _shown.Count - 1; i >= 0; i--)
            {
                var visual = _shown[i];
                if (visual.prompt.IsResolved)
                {
                    visual.fade -= Time.deltaTime;
                    if (visual.fade <= 0f)
                    {
                        Release(visual);
                        continue;
                    }
                }
                else
                {
                    visual.depth = IndexOf(_runner.Open, visual.prompt);
                }

                Refresh(visual, blend);
            }

            var current = _runner.Current;
            if (current != _tokenPrompt)
            {
                _tokenPrompt = current;
                PlaceToken();
                RefreshCount();
            }
        }

        private void Show(QtePlayer player)
        {
            Unbind();
            _player = player;
            _runner = player.CurrentRunner;
            if (_runner == null || anchor == null)
            {
                return;
            }

            _runner.PromptStarted += HandlePromptStarted;
            _runner.PromptResolved += HandlePromptResolved;

            EnsureVisuals();
            SetVisible(true);
        }

        private void Hide(QtePlayer player)
        {
            if (player != _player)
            {
                return;
            }

            Unbind();
            _player = null;
            SetVisible(false);
        }

        private void Unbind()
        {
            if (_runner != null)
            {
                _runner.PromptStarted -= HandlePromptStarted;
                _runner.PromptResolved -= HandlePromptResolved;
                _runner = null;
            }

            for (var i = _shown.Count - 1; i >= 0; i--)
            {
                Release(_shown[i]);
            }

            _tokenPrompt = null;
        }

        // Starts at the back of the stack.
        private void HandlePromptStarted(QtePrompt prompt)
        {
            var visual = _pool.Count > 0 ? _pool.Pop() : CreateVisual();
            visual.prompt = prompt;
            visual.depth = IndexOf(_runner.Open, prompt);
            visual.position = StackPosition(visual.depth);
            visual.scale = StackScale(visual.depth);
            visual.punch = 0f;
            visual.fade = fadeSeconds;
            visual.arc.sprite = RingSprite(_runner.Config.windowDegrees, arcThickness);
            visual.arc.transform.localRotation = Quaternion.Euler(0f, 0f, -prompt.WindowStart);
            visual.ringTint = ringColor;
            visual.arcTint = arcColor;
            visual.needleTint = needleColor;
            visual.root.gameObject.SetActive(true);
            _shown.Add(visual);
            Refresh(visual, 1f);
        }

        private void HandlePromptResolved(QtePrompt prompt)
        {
            var visual = VisualOf(prompt);
            if (visual != null)
            {
                var color = prompt.Hit ? hitColor : missColor;
                visual.arcTint = color;
                visual.needleTint = color;
                if (!prompt.Hit)
                {
                    visual.ringTint = missColor;
                }

                visual.punch = prompt.Hit ? punchSeconds : 0f;
            }

            if (_token != null && prompt == _tokenPrompt)
            {
                if (prompt.Hit)
                {
                    _token.PlayAccepted();
                }
                else
                {
                    _token.PlayWrong();
                }
            }
        }

        private void Refresh(PromptVisual visual, float blend)
        {
            var depth = Mathf.Min(visual.depth, MaxStackDepth);
            visual.position = Vector3.Lerp(visual.position, StackPosition(depth), blend);
            visual.scale = Mathf.Lerp(visual.scale, StackScale(depth), blend);

            var punch = 1f;
            if (visual.punch > 0f)
            {
                visual.punch -= Time.deltaTime;
                punch = Mathf.Lerp(1f, hitPunchScale, Mathf.Clamp01(visual.punch / punchSeconds));
            }

            visual.root.localPosition = visual.position;
            visual.root.localScale = Vector3.one * (visual.scale * punch);
            visual.needle.transform.localRotation = Quaternion.Euler(0f, 0f, -visual.prompt.Angle);

            var alpha = Mathf.Pow(stackFade, depth);
            if (visual.prompt.IsResolved)
            {
                alpha *= Mathf.Clamp01(visual.fade / fadeSeconds);
            }

            visual.ring.color = Faded(visual.ringTint, alpha);
            visual.arc.color = Faded(visual.arcTint, alpha);
            visual.needle.color = Faded(visual.needleTint, alpha);

            // Front rings draw over the ones behind.
            var order = sortingOrder + (MaxStackDepth - depth) * 3;
            visual.ring.sortingOrder = order;
            visual.arc.sortingOrder = order + 1;
            visual.needle.sortingOrder = order + 2;
        }

        private Vector3 StackPosition(int depth)
        {
            return (Vector3)(stackOffset * depth);
        }

        private float StackScale(int depth)
        {
            return Mathf.Pow(stackScale, depth);
        }

        private static Color Faded(Color color, float alpha)
        {
            color.a *= alpha;
            return color;
        }

        private static int IndexOf(IReadOnlyList<QtePrompt> prompts, QtePrompt prompt)
        {
            for (var i = 0; i < prompts.Count; i++)
            {
                if (prompts[i] == prompt)
                {
                    return i;
                }
            }

            return 0;
        }

        private PromptVisual VisualOf(QtePrompt prompt)
        {
            foreach (var visual in _shown)
            {
                if (visual.prompt == prompt)
                {
                    return visual;
                }
            }

            return null;
        }

        private void Release(PromptVisual visual)
        {
            _shown.Remove(visual);
            visual.prompt = null;
            visual.root.gameObject.SetActive(false);
            _pool.Push(visual);
        }

        private void PlaceToken()
        {
            if (_token == null)
            {
                return;
            }

            _token.gameObject.SetActive(_tokenPrompt != null && spec != null);
            if (_tokenPrompt == null || spec == null)
            {
                return;
            }

            spec.TryGetIcon(_runner.Config.token, out var sprite, out var rotation);
            _token.Place(anchor.position, sprite, rotation);
            _token.SetState(InputSetTokenState.Current, spec.ColorFor(InputSetTokenState.Current));
        }

        // "prompt / count" for the front ring.
        private void RefreshCount()
        {
            if (countText != null && _tokenPrompt != null)
            {
                countText.text = $"{_tokenPrompt.Index + 1} / {_runner.Count}";
            }
        }

        private void EnsureVisuals()
        {
            if (_container == null)
            {
                var go = new GameObject("QteRings");
                go.layer = anchor.gameObject.layer;
                go.transform.SetParent(anchor, false);
                _container = go.transform;
            }

            if (_token == null && tokenPrefab != null)
            {
                _token = Instantiate(tokenPrefab, anchor);
            }

            if (countText == null)
            {
                var go = new GameObject("QteCount");
                go.layer = anchor.gameObject.layer;
                go.transform.SetParent(anchor, false);
                go.transform.localPosition = countOffset;
                var label = go.AddComponent<TextMeshPro>();
                label.fontSize = countFontSize;
                label.alignment = TextAlignmentOptions.Center;
                label.color = spec != null ? spec.ColorFor(InputSetTokenState.Current) : Color.white;
                label.sortingOrder = sortingOrder + MaxStackDepth * 3 + 10;
                countText = label;
            }
        }

        private PromptVisual CreateVisual()
        {
            var go = new GameObject("QtePrompt");
            go.layer = _container.gameObject.layer;
            go.transform.SetParent(_container, false);
            var visual = new PromptVisual { root = go.transform };
            visual.ring = CreateRenderer(visual.root, "Ring", RingSprite(360f, ringThickness));
            visual.arc = CreateRenderer(visual.root, "Arc", RingSprite(Qte.Default.windowDegrees, arcThickness));
            visual.needle = CreateRenderer(visual.root, "Needle", NeedleSprite());
            visual.needle.transform.localScale = new Vector3(needleWidth, ringDiameter * 0.5f, 1f);
            return visual;
        }

        // Ring sprites are one world unit across, so scale to the diameter.
        private SpriteRenderer CreateRenderer(Transform parent, string rendererName, Sprite sprite)
        {
            var go = new GameObject(rendererName);
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * ringDiameter;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            return renderer;
        }

        private void SetVisible(bool visible)
        {
            if (_container != null)
            {
                _container.gameObject.SetActive(visible);
            }

            if (_token != null)
            {
                _token.gameObject.SetActive(false);
            }

            if (countText != null)
            {
                countText.gameObject.SetActive(visible);
            }
        }

        // A white anti-aliased ring arc, one world unit across, from 12
        // o'clock clockwise through degrees.
        private static Sprite RingSprite(float degrees, float thickness)
        {
            var key = Mathf.RoundToInt(degrees) * 1000 + Mathf.RoundToInt(thickness * 1000f);
            if (RingSprites.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var size = TextureSize;
            var pixels = new Color32[size * size];
            var outer = size * 0.5f - 1f;
            var inner = outer - Mathf.Max(1f, thickness * size);
            var middle = (outer + inner) * 0.5f;
            var center = (size - 1) * 0.5f;
            var full = degrees >= 359.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    var alpha = Mathf.Clamp01(outer - distance + 0.5f) * Mathf.Clamp01(distance - inner + 0.5f);
                    if (!full && alpha > 0f)
                    {
                        var angle = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
                        if (angle < 0f)
                        {
                            angle += 360f;
                        }

                        var inside = Mathf.Min(angle, degrees - angle);
                        alpha *= Mathf.Clamp01(inside * Mathf.Deg2Rad * middle + 0.5f);
                    }

                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            var sprite = CreateSprite("QteRing", size, size, pixels, new Vector2(0.5f, 0.5f), size);
            RingSprites[key] = sprite;
            return sprite;
        }

        // A white bar one unit long, pivoted at its base.
        private static Sprite NeedleSprite()
        {
            if (_needleSprite != null)
            {
                return _needleSprite;
            }

            var pixels = new Color32[4 * 4];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 255, 255, 255);
            }

            _needleSprite = CreateSprite("QteNeedle", 4, 4, pixels, new Vector2(0.5f, 0f), 4);
            return _needleSprite;
        }

        private static Sprite CreateSprite(string textureName, int width, int height, Color32[] pixels, Vector2 pivot, float pixelsPerUnit)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = textureName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), pivot, pixelsPerUnit);
        }

        private sealed class PromptVisual
        {
            public Transform root;
            public SpriteRenderer ring;
            public SpriteRenderer arc;
            public SpriteRenderer needle;
            public QtePrompt prompt;
            public int depth;
            public Vector3 position;
            public float scale;
            public float punch;
            public float fade;
            public Color ringTint;
            public Color arcTint;
            public Color needleTint;
        }
    }
}
