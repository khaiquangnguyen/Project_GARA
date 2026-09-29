using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GARA.Characters.Gambler
{
    // One coin, die, card, chamber or reel in GambleOutcomeView's row.
    public class GambleToken : MonoBehaviour
    {
        [SerializeField]
        private Image background;

        [SerializeField]
        private TMP_Text label;

        [SerializeField]
        private LayoutElement layout;

        public void Set(Sprite shape, Vector2 size, Color fill, string text, Color textColor, bool raised = false)
        {
            background.sprite = shape;
            background.type = shape != null && shape.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            background.color = fill;
            label.text = text;
            label.color = textColor;
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
            background.rectTransform.anchoredPosition = raised ? new Vector2(0f, 20f) : Vector2.zero;
        }
    }
}
