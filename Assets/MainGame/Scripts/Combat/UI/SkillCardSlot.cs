using GARA.Characters;
using TMPro;
using UnityEngine;

namespace GARA.Combat
{
    // One combat skill slot. Its SpriteRenderer is the frame: size and scale
    // set the bounds, its sorting sets the layer. Hidden unless the slot is
    // selected, when it shows in highlightColor. The card's icon is fit inside
    // the frame and drawn just above it; a card without one shows
    // placeholderIcon and its name instead.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SkillCardSlot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer area;

        [Tooltip("The frame is shown in this color while the slot is selected.")]
        [SerializeField] private Color highlightColor = new(0f, 1f, 0f, 0.5f);

        [Tooltip("Optional. Shown for a card with no icon, under its name.")]
        [SerializeField] private Sprite placeholderIcon;

        [SerializeField] private Color placeholderNameColor = Color.white;

        private SpriteRenderer _icon;
        private TextMeshPro _name;

        private void Reset()
        {
            area = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (area == null)
            {
                area = GetComponent<SpriteRenderer>();
            }

            area.enabled = false;
        }

        public void Show(SkillCardDefinition card)
        {
            if (card == null)
            {
                Clear();
                return;
            }

            var hasIcon = card.icon != null;
            ShowIcon(hasIcon ? card.icon : placeholderIcon);
            ShowName(hasIcon ? null : string.IsNullOrEmpty(card.displayName) ? card.name : card.displayName);
        }

        private void ShowIcon(Sprite sprite)
        {
            if (sprite == null)
            {
                if (_icon != null)
                {
                    _icon.enabled = false;
                }

                return;
            }

            if (_icon == null)
            {
                _icon = new GameObject("Icon").AddComponent<SpriteRenderer>();
                _icon.transform.SetParent(transform, false);
            }

            _icon.sprite = sprite;
            _icon.sortingLayerID = area.sortingLayerID;
            _icon.sortingOrder = area.sortingOrder + 1;
            _icon.enabled = true;
            FitToArea(sprite.bounds);
        }

        public void SetHighlighted(bool highlighted)
        {
            area.color = highlightColor;
            area.enabled = highlighted;
        }

        // Null hides it.
        private void ShowName(string text)
        {
            if (text == null)
            {
                if (_name != null)
                {
                    _name.enabled = false;
                }

                return;
            }

            if (_name == null)
            {
                _name = new GameObject("Name").AddComponent<TextMeshPro>();
                _name.transform.SetParent(transform, false);
                _name.alignment = TextAlignmentOptions.Center;
                _name.enableAutoSizing = true;
                _name.fontSizeMin = 0.5f;
                _name.fontSizeMax = 8f;
                _name.fontStyle = FontStyles.Bold;
                _name.outlineWidth = 0.2f;
                _name.outlineColor = Color.black;
            }

            var target = AreaBounds();
            _name.rectTransform.sizeDelta = target.size * 0.9f;
            _name.transform.localPosition = target.center;
            _name.sortingLayerID = area.sortingLayerID;
            _name.sortingOrder = area.sortingOrder + 2;
            _name.color = placeholderNameColor;
            _name.text = text;
            _name.enabled = true;
        }

        public void Clear()
        {
            if (_icon != null)
            {
                _icon.enabled = false;
            }

            if (_name != null)
            {
                _name.enabled = false;
            }
        }

        // Uniform scale so the icon fits the frame, in this transform's
        // local space.
        private void FitToArea(Bounds bounds)
        {
            var target = AreaBounds();
            var scale = Mathf.Min(
                bounds.size.x > 0f ? target.size.x / bounds.size.x : 1f,
                bounds.size.y > 0f ? target.size.y / bounds.size.y : 1f);
            _icon.transform.localScale = Vector3.one * scale;
            _icon.transform.localPosition = new Vector3(
                target.center.x - bounds.center.x * scale,
                target.center.y - bounds.center.y * scale,
                0f);
        }

        // From the sprite rather than the renderer, which is disabled.
        private Bounds AreaBounds()
        {
            var sprite = area.sprite;
            if (sprite == null)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            if (area.drawMode == SpriteDrawMode.Simple)
            {
                return sprite.bounds;
            }

            var pivot = sprite.pivot / sprite.rect.size;
            var center = (new Vector2(0.5f, 0.5f) - pivot) * area.size;
            return new Bounds(center, area.size);
        }
    }
}
