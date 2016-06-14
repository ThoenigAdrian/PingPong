using InputFunctionality.KeyboardAdapter;
using PingPongClient.InputLayer.InputTranslation;
using System;

namespace PingPongClient.InputLayer.KeyboardInputs
{
    public enum PlayerMovementInputs
    {
        StopMoving,
        Up,
        Down,
        NoInput
    }

    class PlayerInput : InputInterfaceKeyboard
    {
        public int ID { get; set; }

        MovementTranslation Translation { get; set; }

        public PlayerInput(KeyboardAdvanced keyboard, int playerID, int playerIndex) : base(keyboard)
        {
            Translation = TranslationFactory.GetTranslationForPlayerIndex(playerIndex);
            ID = playerID;
        }

        public PlayerInputs GetPlayerInput()
        {
            PlayerInputs input = new PlayerInputs();
            input.ID = ID;
            input.MovementInput = GetMovementInput();

            return input;
        }

        private PlayerMovementInputs GetMovementInput()
        {
            if (Translation == null)
                return PlayerMovementInputs.NoInput;

            foreach (PlayerMovementInputs movement in Enum.GetValues(typeof(PlayerMovementInputs)))
            {
                if (Keyboard.KeyNowPressed(Translation.GetMovementKey(movement)))
                    return movement;
            }

            if (Keyboard.KeyNowReleased(Translation.GetMovementKey(PlayerMovementInputs.Up))
                || Keyboard.KeyNowReleased(Translation.GetMovementKey(PlayerMovementInputs.Down)))
                return PlayerMovementInputs.StopMoving;

            return PlayerMovementInputs.NoInput;
        }
    }
}
