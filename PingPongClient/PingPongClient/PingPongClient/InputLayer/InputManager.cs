using InputFunctionality.KeyboardAdapter;
using PingPongClient.InputLayer.KeyboardInputs;
using System.Collections.Generic;

namespace PingPongClient.InputLayer
{
    class PlayerInputs
    {
        public int ID { get; set; }

        public PlayerMovementInputs MovementInput = PlayerMovementInputs.NoInput;
    }

    enum PlayerMovementInputs
    {
        StopMoving,
        Up,
        Down,
        NoInput
    }

    enum ControlInputs
    {
        Pause,
        Quit,
        Restart,
        NoInput
    }

    enum TextEditInputs
    {
        Number,
        Delete,
        Enter,
        NoInput
    }

    class InputManager
    {
        KeyboardAdvanced KeyboardAdapter = new KeyboardAdvanced();

        ControlInput ControlInput;
        List<PlayerInput> PlayerInputs;
        TextEditInput TextInput;

        public InputManager()
        {
            ControlInput = new ControlInput(KeyboardAdapter);
            PlayerInputs = new List<PlayerInput>();
            TextInput = new TextEditInput(KeyboardAdapter);
        }

        public void Initialize()
        {
            KeyboardAdapter.Initialize();
        }

        public void Update()
        {
            KeyboardAdapter.UpdateState();
        }

        public void AddPlayerInput(int playerID, int playerIndex)
        {
            PlayerInputs.Add(new PlayerInput(KeyboardAdapter, playerID, playerIndex));
        }

        public PlayerInputs[] GetMovementInput()
        {
            List<PlayerInputs> allPlayerInputs = new List<PlayerInputs>();

            foreach (PlayerInput input in PlayerInputs)
            {
                allPlayerInputs.Add(input.GetPlayerInput());
            }

            return allPlayerInputs.ToArray();
        }

        public ControlInputs GetControlInput()
        {
            return ControlInput.GetControlInput();
        }

        public TextEditInputs GetTextEditInput()
        {
            return TextInput.GetTextEditInputs();
        }

        public string GetNumberInput()
        {
            return TextInput.GetPressedKeyString();
        }
    }
}
