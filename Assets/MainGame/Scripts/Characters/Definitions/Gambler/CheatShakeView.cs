using GARA.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GARA.Characters.Gambler
{
    // Shows any CheatShakePlayer's run: the count against the required
    // number, the next key and the time left. Lives in the scene.
    public class CheatShakeView : MonoBehaviour
    {
        [Tooltip("Enabled while a run plays.")]
        [SerializeField]
        private GameObject content;

        [Tooltip("\"3 / 8\" while playing; the required number before the first press.")]
        [SerializeField]
        private TMP_Text countText;

        [Tooltip("Optional. The key that counts next.")]
        [SerializeField]
        private TMP_Text nextKeyText;

        [Tooltip("Optional. Filled image that drains with the time left.")]
        [SerializeField]
        private Image timeFill;

        private CheatShakePlayer _player;

        private void OnEnable()
        {
            CheatShakePlayer.AnyStarted += Show;
            CheatShakePlayer.AnyEnded += Hide;
            SetVisible(false);
        }

        private void OnDisable()
        {
            CheatShakePlayer.AnyStarted -= Show;
            CheatShakePlayer.AnyEnded -= Hide;
            _player = null;
        }

        private void Update()
        {
            if (_player != null)
            {
                Refresh();
            }
        }

        private void Show(CheatShakePlayer player)
        {
            _player = player;
            SetVisible(true);
            Refresh();
        }

        private void Hide(CheatShakePlayer player)
        {
            if (player != _player)
            {
                return;
            }

            _player = null;
            SetVisible(false);
        }

        private void Refresh()
        {
            var runner = _player.CurrentRunner;
            if (runner == null)
            {
                return;
            }

            if (countText != null)
            {
                countText.text = runner.HasStarted ? $"{runner.Count} / {runner.Required}" : runner.Required.ToString();
            }

            if (nextKeyText != null)
            {
                var config = runner.Config;
                nextKeyText.text = runner.NextSide < 0 ? KeyName(config.leftToken)
                    : runner.NextSide > 0 ? KeyName(config.rightToken)
                    : $"{KeyName(config.leftToken)} / {KeyName(config.rightToken)}";
            }

            if (timeFill != null)
            {
                timeFill.fillAmount = runner.TimeLeft01;
            }
        }

        private string KeyName(InputToken token)
        {
            var map = _player.InputMap;
            return map != null && map.TryGetDisplayName(token, out var name) ? name : token.ToString();
        }

        private void SetVisible(bool visible)
        {
            if (content != null)
            {
                content.SetActive(visible);
            }
        }
    }
}
