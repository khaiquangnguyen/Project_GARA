using GARA.Input;
using TMPro;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Shows the pull / chicken out prompt while any
    // DuelRouletteDecisionPlayer is asking. Lives in the scene.
    public class DuelRouletteDecisionView : MonoBehaviour
    {
        [Tooltip("Enabled while asking.")]
        [SerializeField]
        private GameObject content;

        [Tooltip("Optional. The keys and the next pull's fire chance.")]
        [SerializeField]
        private TMP_Text prompt;

        private void OnEnable()
        {
            DuelRouletteDecisionPlayer.AnyDecisionStarted += Show;
            DuelRouletteDecisionPlayer.AnyDecisionEnded += Hide;
            SetVisible(false);
        }

        private void OnDisable()
        {
            DuelRouletteDecisionPlayer.AnyDecisionStarted -= Show;
            DuelRouletteDecisionPlayer.AnyDecisionEnded -= Hide;
        }

        private void Show(DuelRouletteDecisionPlayer player)
        {
            if (prompt != null)
            {
                var round = player.Round;
                prompt.text = $"{round.EmptyClicks} click(s) — {round.NextFireChance:P0} it fires\n"
                              + $"[{KeyName(player, player.PullToken)}] Pull    [{KeyName(player, player.ChickenOutToken)}] Chicken out";
            }

            SetVisible(true);
        }

        private void Hide(DuelRouletteDecisionPlayer player)
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (content != null)
            {
                content.SetActive(visible);
            }
        }

        private static string KeyName(DuelRouletteDecisionPlayer player, InputToken token)
        {
            return player.InputMap != null && player.InputMap.TryGetDisplayName(token, out var name) ? name : token.ToString();
        }
    }
}
