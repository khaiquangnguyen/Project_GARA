using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Asks the player to pull again or chicken out on the Gambler's Duel
    // Roulette turn. Scene visuals present it through the static events.
    public class DuelRouletteDecisionPlayer : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        [SerializeField]
        private InputToken pullToken = new InputToken(1);

        [SerializeField]
        private InputToken chickenOutToken = new InputToken(3);

        private InputTokenPoller _poller;
        private Action<InputToken> _onTokenPressed;
        private Action<bool> _onDecided;

        public static event Action<DuelRouletteDecisionPlayer> AnyDecisionStarted;
        public static event Action<DuelRouletteDecisionPlayer> AnyDecisionEnded;

        public bool IsAsking => _onDecided != null;

        // The round being decided on; null when not asking.
        public DuelRouletteOutcome Round { get; private set; }

        public InputTokenMap InputMap => inputMap;

        public InputToken PullToken => pullToken;

        public InputToken ChickenOutToken => chickenOutToken;

        // onDecided gets true to pull, false to chicken out.
        public void Ask(DuelRouletteOutcome round, Action<bool> onDecided)
        {
            Cancel();
            _poller = new InputTokenPoller(inputMap);
            inputMap?.EnableAll();
            Round = round;
            _onDecided = onDecided;
            AnyDecisionStarted?.Invoke(this);
        }

        // Stops asking without deciding.
        public void Cancel()
        {
            if (IsAsking)
            {
                End();
            }
        }

        private void Awake()
        {
            _onTokenPressed = HandleTokenPressed;
        }

        private void Update()
        {
            if (IsAsking)
            {
                _poller.Poll(_onTokenPressed, null);
            }
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void HandleTokenPressed(InputToken token)
        {
            if (!IsAsking)
            {
                return;
            }

            if (token == pullToken)
            {
                Decide(true);
            }
            else if (token == chickenOutToken)
            {
                Decide(false);
            }
        }

        private void Decide(bool pull)
        {
            var onDecided = _onDecided;
            End();
            onDecided?.Invoke(pull);
        }

        private void End()
        {
            _onDecided = null;
            AnyDecisionEnded?.Invoke(this);
            Round = null;
        }
    }
}
