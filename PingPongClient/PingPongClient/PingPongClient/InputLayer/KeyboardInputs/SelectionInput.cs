using InputFunctionality.KeyboardAdapter;
using PingPongClient.InputLayer.InputTranslation;
using System;

namespace PingPongClient.InputLayer.KeyboardInputs
{ 
    public enum SelectionInputs
    {
        Select,
        Up,
        Down,
        Left,
        Right,
        NoInput
    }

    class SelectionInput : InputInterfaceKeyboard
    {
        SelectionTranslation Translation { get; set; }

        public SelectionInput(KeyboardAdvanced keyboard) : base(keyboard)
        {
            Translation = new SelectionTranslation();
        }

        public SelectionInputs GetSelectionInput()
        {
            if (Translation == null)
                return SelectionInputs.NoInput;

            foreach (SelectionInputs selection in Enum.GetValues(typeof(SelectionInputs)))
            {
                if (Keyboard.KeyNowPressed(Translation.GetSelectionKey(selection)))
                    return selection;
            }

            return SelectionInputs.NoInput;
        }
    }
}
