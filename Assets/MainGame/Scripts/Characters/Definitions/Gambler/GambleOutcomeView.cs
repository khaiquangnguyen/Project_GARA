using System.Collections.Generic;
using GARA.Characters;
using TMPro;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Shows every Gambler roll (GambleEvents) as a row of tokens: coins,
    // dice, revolver chambers, barrage shots, slot reels. Lives in the scene.
    public class GambleOutcomeView : MonoBehaviour
    {
        [Tooltip("Enabled while a roll is shown.")]
        [SerializeField]
        private GameObject content;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text resultText;

        [Tooltip("Enabled when the roll was cheated.")]
        [SerializeField]
        private GameObject cheatBadge;

        [SerializeField]
        private RectTransform tokenRow;

        [Tooltip("Inactive template, copied once per token.")]
        [SerializeField]
        private GambleToken tokenTemplate;

        [SerializeField]
        private Sprite roundSprite;

        [SerializeField]
        private Sprite squareSprite;

        [Tooltip("Seconds a finished roll stays up.")]
        [Min(0f)]
        [SerializeField]
        private float holdSeconds = 2.5f;

        [Header("Colors")]
        [SerializeField] private Color gold = new Color32(0xD9, 0xA4, 0x41, 0xFF);
        [SerializeField] private Color ivory = new Color32(0xF4, 0xEC, 0xD8, 0xFF);
        [SerializeField] private Color dim = new Color32(0x3B, 0x40, 0x3E, 0xFF);
        [SerializeField] private Color ink = new Color32(0x1B, 0x17, 0x10, 0xFF);
        [SerializeField] private Color red = new Color32(0xC8, 0x41, 0x3B, 0xFF);

        [Header("Dice")]
        [Tooltip("GARA/Die material. Set: six-sided dice roll as 3D cubes; unset: flat tokens.")]
        [SerializeField]
        private Material dieMaterial;

        [Tooltip("Die edge, in canvas pixels.")]
        [Min(1f)]
        [SerializeField]
        private float dieSize = 80f;

        [Tooltip("Seconds a die tumbles before landing; keep under the Gambler's outcome reveal.")]
        [Min(0.05f)]
        [SerializeField]
        private float dieRollSeconds = 0.9f;

        [Tooltip("Seconds between each die starting its roll.")]
        [Min(0f)]
        [SerializeField]
        private float dieStagger = 0.08f;

        [Tooltip("First bounce height, in canvas pixels.")]
        [Min(0f)]
        [SerializeField]
        private float dieBounce = 70f;

        private readonly List<GambleToken> _tokens = new List<GambleToken>();
        private readonly List<DiceCube> _dice = new List<DiceCube>();
        private DiceOutcome _rollingDice;
        private float _diceLandAt;
        private float _hideAt;

        private void OnEnable()
        {
            GambleEvents.Rolled += ShowRoll;
            GambleEvents.SlotSpun += ShowSpin;
            if (tokenTemplate != null)
            {
                tokenTemplate.gameObject.SetActive(false);
            }

            SetVisible(false);
        }

        private void OnDisable()
        {
            GambleEvents.Rolled -= ShowRoll;
            GambleEvents.SlotSpun -= ShowSpin;
        }

        private void Update()
        {
            if (_rollingDice != null && Time.time >= _diceLandAt)
            {
                LandDice(_rollingDice);
            }

            if (content != null && content.activeSelf && Time.time >= _hideAt)
            {
                SetVisible(false);
            }
        }

        private void ShowRoll(GamblerSkillCard card, GambleOutcome outcome, bool cheated)
        {
            if (card != null && !card.showOutcome)
            {
                return;
            }

            Begin(card != null && !string.IsNullOrEmpty(card.displayName) ? card.displayName : card != null ? card.name : "Gamble", cheated);
            switch (outcome)
            {
                case CoinTossOutcome coins:
                    ShowCoins(coins);
                    break;
                case DiceOutcome dice:
                    ShowDice(dice);
                    break;
                case RouletteBarrageOutcome barrage:
                    ShowBarrage(barrage);
                    break;
                case RussianRouletteOutcome roulette:
                    ShowRoulette(roulette);
                    break;
                default:
                    ClearTokens();
                    resultText.text = outcome?.ToString() ?? "";
                    break;
            }
        }

        private void ShowSpin(ICombatTarget spinner, IReadOnlyList<SlotSymbol> spin, bool jackpot)
        {
            Begin("Slot Machine", false);
            ClearTokens();
            foreach (var symbol in spin)
            {
                AddToken(squareSprite, new Vector2(120f, 140f), jackpot ? gold : ivory, symbol.symbolName, ink);
            }

            resultText.text = jackpot ? "JACKPOT!" : "";
            resultText.color = jackpot ? gold : ivory;
        }

        private void ShowCoins(CoinTossOutcome coins)
        {
            ClearTokens();
            foreach (var heads in coins.Faces)
            {
                AddToken(roundSprite, new Vector2(110f, 110f), heads ? gold : dim, heads ? "H" : "T", heads ? ink : ivory, heads);
            }

            resultText.text = $"{coins.HeadsCount} / {coins.Faces.Count} heads";
        }

        private void ShowDice(DiceOutcome dice)
        {
            ClearTokens();
            if (dieMaterial != null && dice.Sides == 6)
            {
                RollDice(dice);
                return;
            }

            foreach (var face in dice.Faces)
            {
                AddToken(squareSprite, new Vector2(110f, 110f), ivory, face.ToString(), ink, face == dice.Sides);
            }

            resultText.text = $"Total {dice.Total}";
        }

        // A 3D die per face in the token row; the total shows once they land.
        private void RollDice(DiceOutcome dice)
        {
            var canvas = tokenRow.GetComponentInParent<Canvas>().rootCanvas;
            for (var i = 0; i < dice.Faces.Count; i++)
            {
                AddToken(null, new Vector2(dieSize * 1.3f, dieSize * 1.3f), Color.clear, "", Color.clear);
                var die = DiceCube.Create(_tokens[_tokens.Count - 1].transform, dieMaterial, dieSize);
                die.SetSorting(canvas.sortingLayerID, canvas.sortingOrder + 1);
                die.Roll(dice.Faces[i], new Vector3(0f, 0f, -dieSize), dieRollSeconds, i * dieStagger, dieBounce);
                _dice.Add(die);
            }

            _rollingDice = dice;
            _diceLandAt = Time.time + dieRollSeconds + (dice.Faces.Count - 1) * dieStagger;
            _hideAt = _diceLandAt + holdSeconds;
            resultText.text = "Rolling...";
        }

        private void LandDice(DiceOutcome dice)
        {
            _rollingDice = null;
            for (var i = 0; i < _dice.Count && i < dice.Faces.Count; i++)
            {
                if (_dice[i] != null && dice.Faces[i] == dice.Sides)
                {
                    _dice[i].SetTint(gold);
                }
            }

            resultText.text = $"Total {dice.Total}";
        }

        private void ShowRoulette(RussianRouletteOutcome roulette)
        {
            ClearTokens();
            for (var chamber = 0; chamber < roulette.Chambers; chamber++)
            {
                if (chamber == roulette.Chamber)
                {
                    AddToken(roundSprite, new Vector2(80f, 80f), roulette.Fired ? red : ivory, roulette.Fired ? "!" : "", ink, true);
                    continue;
                }

                AddToken(roundSprite, new Vector2(64f, 64f), dim, "", ivory);
            }

            resultText.text = $"{(roulette.Fired ? "BANG!" : "click")}   {roulette.Loaded} / {roulette.Chambers} loaded";
        }

        // One chamber per shot, in firing order; the ones that fired are raised.
        private void ShowBarrage(RouletteBarrageOutcome barrage)
        {
            ClearTokens();
            foreach (var fired in barrage.Fired)
            {
                AddToken(roundSprite, new Vector2(72f, 72f), fired ? red : dim, fired ? "!" : "", fired ? ink : ivory, fired);
            }

            resultText.text = $"{barrage.Hits} / {barrage.Fired.Count} fired   {barrage.Loaded} / {barrage.Chambers} loaded";
        }

        private void Begin(string title, bool cheated)
        {
            titleText.text = title;
            resultText.color = ivory;
            if (cheatBadge != null)
            {
                cheatBadge.SetActive(cheated);
            }

            _hideAt = Time.time + holdSeconds;
            SetVisible(true);
        }

        private void AddToken(Sprite shape, Vector2 size, Color fill, string text, Color textColor, bool raised = false)
        {
            var token = Instantiate(tokenTemplate, tokenRow);
            token.gameObject.SetActive(true);
            token.Set(shape, size, fill, text, textColor, raised);
            _tokens.Add(token);
        }

        private void ClearTokens()
        {
            foreach (var token in _tokens)
            {
                if (token != null)
                {
                    Destroy(token.gameObject);
                }
            }

            _tokens.Clear();
            _dice.Clear();
            _rollingDice = null;
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
