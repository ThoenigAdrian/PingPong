using InputFunctionality.KeyboardAdapter;
using PingPongClient.InputLayer.InputTranslation;
using System;

namespace PingPongClient.InputLayer
{
    class ControlInput : InputInterfaceKeyboard
    {
        ControlTranslation Translation { get; set; }

        public ControlInput(KeyboardAdvanced keyboard) : base (keyboard)
        {
            Translation = new ControlTranslation();
        }

        public ControlInputs GetControlInput()
        {
            if (Translation == null)
                return ControlInputs.NoInput;

            foreach (ControlInputs control in Enum.GetValues(typeof(ControlInputs)))
            {
                if (Keyboard.KeyNowPressed(Translation.GetControlKey(control)))
                    return control;
            }

            return ControlInputs.NoInput;
        }
    }
}
