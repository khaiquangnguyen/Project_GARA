using System;
using TMPro;
using UnityEngine;

namespace GARA.Combat
{
    // A player's HP/MP bar: each fill is clipped by a rectangular SpriteMask
    // child that slides off it as the value falls, next to its number.
    public class StatusBarView : MonoBehaviour
    {
        [Serializable]
        private class Gauge
        {
            public SpriteRenderer fill;
            public SpriteMask mask;
            public TMP_Text number;

            [Range(0f, 1f)]
            public float fraction = 1f;

            public void Set(int current, int max, bool drainTowardIcons)
            {
                fraction = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
                if (number != null)
                {
                    number.SetText("{0}", Mathf.Max(0, current));
                }

                Apply(drainTowardIcons);
            }

            // The mask is a 1x1-unit square child of the fill, so its local
            // space is the fill sprite's.
            public void Apply(bool drainTowardIcons)
            {
                if (fill == null || fill.sprite == null || mask == null)
                {
                    return;
                }

                var bounds = fill.sprite.bounds;
                var shift = (1f - fraction) * bounds.size.x * (drainTowardIcons ? 1f : -1f);
                mask.transform.localPosition = new Vector3(bounds.center.x + shift, bounds.center.y, 0f);
                mask.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
            }
        }

        [SerializeField] private Gauge hp = new();
        [SerializeField] private Gauge mp = new();

        [Tooltip("On, the fills empty from the number end toward the icons.")]
        [SerializeField] private bool drainTowardIcons = true;

        private void OnValidate()
        {
            hp.Apply(drainTowardIcons);
            mp.Apply(drainTowardIcons);
        }

        public void SetHp(int current, int max)
        {
            hp.Set(current, max, drainTowardIcons);
        }

        public void SetMp(int current, int max)
        {
            mp.Set(current, max, drainTowardIcons);
        }
    }
}
