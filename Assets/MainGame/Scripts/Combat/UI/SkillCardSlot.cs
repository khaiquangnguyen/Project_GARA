using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // One combat skill slot. Its SpriteRenderer is the frame: size and scale
    // set the bounds, its sorting sets the layer. Hidden unless the slot is
    // selected, when it shows in highlightColor. The card's icon is fit inside
    // the frame and drawn just above it.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SkillCardSlot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer area;

        [Tooltip("The frame is shown in this color while the slot is selected.")]
        [SerializeField] private Color highlightColor = new(0f, 1f, 0f, 0.5f);

        private SpriteRenderer _icon;

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
            var sprite = card != null ? card.icon : null;
            if (sprite == null)
            {
                Clear();
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

        public void Clear()
        {
            if (_icon != null)
            {
                _icon.enabled = false;
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
