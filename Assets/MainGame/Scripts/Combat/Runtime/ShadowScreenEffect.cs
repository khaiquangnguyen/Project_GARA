using Spine.Unity;
using UnityEngine;

namespace GARA.Combat
{
    // Root of a card's shadow screen: black drapes close across the stage,
    // then a backlit screen slowly lights up behind the performer, who turns
    // into a black silhouette on it. Hit() fades the screen out and pulls
    // the drapes open, then it destroys itself.
    public class ShadowScreenEffect : MonoBehaviour
    {
        // Seconds it waits for Hit() before closing anyway, in case the card
        // never reports one.
        private const float HitTimeout = 30f;

        private const int FoldTextureWidth = 256;
        private const int ScreenTextureSize = 64;

        [Header("Drapes")]
        [SerializeField]
        private Color drapeColor = new Color(0.06f, 0.04f, 0.07f, 1f);

        [Tooltip("Vertical folds per drape.")]
        [Min(1)]
        [SerializeField]
        private int folds = 7;

        [Tooltip("How much darker the fold shadows are than the fold tops.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float foldShading = 0.55f;

        [Tooltip("Seconds the drapes take to close.")]
        [Min(0f)]
        [SerializeField]
        private float closeDuration = 0.5f;

        [Tooltip("Seconds the drapes take to pull open at the end.")]
        [Min(0f)]
        [SerializeField]
        private float openDuration = 0.35f;

        [Header("Screen")]
        [Tooltip("Colour of the lit screen.")]
        [SerializeField]
        private Color screenColor = new Color(1f, 0.92f, 0.75f, 1f);

        [Tooltip("How much darker the screen's edges are than its lit centre.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float screenVignette = 0.45f;

        [Tooltip("World units of screen around the performer's body.")]
        [SerializeField]
        private Vector2 screenPadding = new Vector2(1.2f, 0.6f);

        [Tooltip("The performer's tint while behind the screen.")]
        [SerializeField]
        private Color silhouetteColor = Color.black;

        [Tooltip("Seconds between the drapes closing and the screen starting to light.")]
        [Min(0f)]
        [SerializeField]
        private float screenDelay = 0.15f;

        [Tooltip("Seconds the screen takes to light up.")]
        [Min(0f)]
        [SerializeField]
        private float screenFadeIn = 0.8f;

        [Tooltip("Seconds it stays after the hit before the screen goes dark.")]
        [Min(0f)]
        [SerializeField]
        private float holdAfterHit = 0.1f;

        [Tooltip("Seconds the screen takes to go dark before the drapes open.")]
        [Min(0f)]
        [SerializeField]
        private float screenFadeOut = 0.25f;

        [Tooltip("Sorting order of the drapes; the screen and performer draw just above them.")]
        [SerializeField]
        private int sortingOrder = 2000;

        private SpriteRenderer _left;
        private SpriteRenderer _right;
        private SpriteRenderer _screen;
        private Sprite _drapeSprite;
        private Sprite _screenSprite;
        private Texture2D _foldTexture;
        private Texture2D _screenTexture;
        private Transform _anchor;
        private Vector3 _screenOffset;
        private SkeletonRenderer _skeleton;
        private MeshRenderer _performerRenderer;
        private int _performerSortingOrder;
        private Color _performerColor;
        private float _elapsed;
        private float _sinceHit;
        private bool _hasHit;
        private bool _shown;
        private bool _performerOnScreen;

        // Seconds from Show until the screen is fully lit — when the card's
        // moves should start.
        public float IntroDuration => closeDuration + screenDelay + screenFadeIn;

        // Covers body (the performer's world bounds) with the screen,
        // following the performer's root as it moves.
        public void Show(GameObject performer, Bounds body)
        {
            _anchor = performer.transform;
            _screenOffset = body.center - _anchor.position;
            _elapsed = 0f;
            _sinceHit = 0f;
            _hasHit = false;

            _skeleton = performer.GetComponentInChildren<SkeletonRenderer>();
            _performerRenderer = _skeleton != null ? _skeleton.GetComponent<MeshRenderer>() : null;
            var sortingLayerId = _performerRenderer != null ? _performerRenderer.sortingLayerID : 0;
            if (_skeleton != null && _skeleton.Skeleton != null)
            {
                var color = _skeleton.Skeleton.GetColor();
                _performerColor = new Color(color.r, color.g, color.b, color.a);
            }

            BuildSprites();
            _left = SpawnRenderer("Left Drape", _drapeSprite, sortingLayerId, sortingOrder);
            _right = SpawnRenderer("Right Drape", _drapeSprite, sortingLayerId, sortingOrder);
            _right.flipX = true;
            _screen = SpawnRenderer("Screen", _screenSprite, sortingLayerId, sortingOrder + 1);
            _screen.transform.localScale = new Vector3(body.size.x + screenPadding.x * 2f, body.size.y + screenPadding.y * 2f, 1f);
            _shown = true;
            LateUpdate();
        }

        public void Hit()
        {
            _hasHit = true;
        }

        private void LateUpdate()
        {
            if (!_shown)
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

            var closed = closeDuration > 0f ? EaseOutCubic(Mathf.Clamp01(_elapsed / closeDuration)) : 1f;
            var lit = screenFadeIn > 0f ? Mathf.Clamp01((_elapsed - closeDuration - screenDelay) / screenFadeIn) : _elapsed >= closeDuration + screenDelay ? 1f : 0f;
            // The performer steps onto the screen once the drapes hide it.
            var onScreen = _elapsed >= closeDuration;

            if (_hasHit)
            {
                var dimming = _sinceHit - holdAfterHit;
                if (dimming > 0f)
                {
                    var dark = screenFadeOut > 0f ? Mathf.Clamp01(dimming / screenFadeOut) : 1f;
                    lit *= 1f - dark;
                    if (dark >= 1f)
                    {
                        onScreen = false;
                        var open = openDuration > 0f ? Mathf.Clamp01((dimming - screenFadeOut) / openDuration) : 1f;
                        if (open >= 1f)
                        {
                            Destroy(gameObject);
                            return;
                        }

                        closed *= 1f - EaseInCubic(open);
                    }
                }
            }

            SetPerformerOnScreen(onScreen);
            var screen = screenColor;
            screen.a *= lit;
            _screen.color = screen;

            var camera = Camera.main;
            if (camera != null)
            {
                var halfHeight = camera.orthographicSize * 1.1f;
                var halfWidth = camera.orthographicSize * camera.aspect * 1.1f;
                var centre = camera.transform.position;
                // Each drape covers half the view when closed, starting just
                // off its own edge.
                PlaceDrape(_left, Mathf.Lerp(centre.x - halfWidth * 1.5f, centre.x - halfWidth * 0.5f, closed), centre.y, halfWidth, halfHeight * 2f);
                PlaceDrape(_right, Mathf.Lerp(centre.x + halfWidth * 1.5f, centre.x + halfWidth * 0.5f, closed), centre.y, halfWidth, halfHeight * 2f);
            }

            if (_anchor != null)
            {
                var at = _anchor.position + _screenOffset;
                _screen.transform.SetPositionAndRotation(new Vector3(at.x, at.y, 0f), Quaternion.identity);
            }
        }

        // On screen: a silhouette drawn above the drapes. Off: as it was.
        private void SetPerformerOnScreen(bool onScreen)
        {
            if (onScreen == _performerOnScreen)
            {
                return;
            }

            _performerOnScreen = onScreen;
            if (_skeleton != null && _skeleton.Skeleton != null)
            {
                var tint = onScreen ? silhouetteColor : _performerColor;
                // Keeps whatever alpha others (e.g. a fade) have set.
                _skeleton.Skeleton.SetColor(tint.r, tint.g, tint.b, _skeleton.Skeleton.GetColor().a);
            }

            if (_performerRenderer == null)
            {
                return;
            }

            if (onScreen)
            {
                _performerSortingOrder = _performerRenderer.sortingOrder;
                _performerRenderer.sortingOrder = sortingOrder + 2;
            }
            // Unless the combat has re-sorted it since.
            else if (_performerRenderer.sortingOrder == sortingOrder + 2)
            {
                _performerRenderer.sortingOrder = _performerSortingOrder;
            }
        }

        private void PlaceDrape(SpriteRenderer drape, float x, float y, float width, float height)
        {
            drape.color = drapeColor;
            drape.transform.SetPositionAndRotation(new Vector3(x, y, 0f), Quaternion.identity);
            // The drape sprite is one texel tall.
            drape.transform.localScale = new Vector3(width, height * FoldTextureWidth, 1f);
        }

        private SpriteRenderer SpawnRenderer(string objectName, Sprite sprite, int sortingLayerId, int order)
        {
            var rendererObject = new GameObject(objectName);
            rendererObject.transform.SetParent(transform, false);
            var spriteRenderer = rendererObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingLayerID = sortingLayerId;
            spriteRenderer.sortingOrder = order;
            return spriteRenderer;
        }

        // 1-unit-wide sprites: a drape with vertical folds, and a screen lit
        // brightest at its centre.
        private void BuildSprites()
        {
            _foldTexture = new Texture2D(FoldTextureWidth, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (var x = 0; x < FoldTextureWidth; x++)
            {
                var wave = 0.5f + 0.5f * Mathf.Cos((float)x / FoldTextureWidth * folds * Mathf.PI * 2f);
                var shade = Mathf.Lerp(1f - foldShading, 1f, wave);
                _foldTexture.SetPixel(x, 0, new Color(shade, shade, shade, 1f));
            }

            _foldTexture.Apply();
            _drapeSprite = Sprite.Create(_foldTexture, new Rect(0f, 0f, FoldTextureWidth, 1f), new Vector2(0.5f, 0.5f), FoldTextureWidth, 0, SpriteMeshType.FullRect);

            _screenTexture = new Texture2D(ScreenTextureSize, ScreenTextureSize, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var half = ScreenTextureSize * 0.5f;
            for (var y = 0; y < ScreenTextureSize; y++)
            {
                for (var x = 0; x < ScreenTextureSize; x++)
                {
                    var offset = new Vector2((x + 0.5f - half) / half, (y + 0.5f - half) / half);
                    var shade = 1f - screenVignette * Mathf.Clamp01(offset.sqrMagnitude * 0.5f);
                    _screenTexture.SetPixel(x, y, new Color(shade, shade, shade, 1f));
                }
            }

            _screenTexture.Apply();
            _screenSprite = Sprite.Create(_screenTexture, new Rect(0f, 0f, ScreenTextureSize, ScreenTextureSize), new Vector2(0.5f, 0.5f), ScreenTextureSize);
        }

        // Hands the performer back as it was.
        private void OnDestroy()
        {
            if (_shown)
            {
                SetPerformerOnScreen(false);
            }

            Destroy(_drapeSprite);
            Destroy(_screenSprite);
            Destroy(_foldTexture);
            Destroy(_screenTexture);
        }

        private static float EaseOutCubic(float t)
        {
            var u = 1f - t;
            return 1f - u * u * u;
        }

        private static float EaseInCubic(float t)
        {
            return t * t * t;
        }
    }
}
