using InputFunctionality.KeyboardAdapter;
using PingPongClient.InputLayer.KeyboardInputs;
using System.Collections.Generic;

namespace PingPongClient.InputLayer
{
    public class PlayerInputs
    {
        public int ID { get; set; }

        public PlayerMovementInputs MovementInput = PlayerMovementInputs.NoInput;
    }

    public class InputManager
    {
        KeyboardAdvanced KeyboardAdapter = new KeyboardAdvanced();

        ControlInput ControlInput;
        List<PlayerInput> PlayerInputs;
        TextEditInput TextInput;
        SelectionInput SelectInput;

        public InputManager()
        {
            ControlInput = new ControlInput(KeyboardAdapter);
            PlayerInputs = new List<PlayerInput>();
            TextInput = new TextEditInput(KeyboardAdapter);
            SelectInput = new SelectionInput(KeyboardAdapter);
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

        public void ClearPlayerInput()
        {
            PlayerInputs.Clear();
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

        public string GetNumberInputAsString()
        {
            return TextInput.GetPressedKeyString();
        }

        public int GetNumberInput()
        {
            return TextInput.GetPressedKeyValue();
        }

        public SelectionInputs GetSelectionInput()
        {
            return SelectInput.GetSelectionInput();
        }
    }
}
