using InputFunctionality.KeyboardAdapter;
using Microsoft.Xna.Framework.Input;
using PingPongClient.InputLayer.InputTranslation;
using System;

namespace PingPongClient.InputLayer.KeyboardInputs
{
    class TextEditInput : InputInterfaceKeyboard
    {
        TextEditTranslation Translation;

        public TextEditInput(KeyboardAdvanced keyboard) : base (keyboard)
        {
            Translation = new TextEditTranslation();
        }

        public TextEditInputs GetTextEditInputs()
        {
            if (Translation != null)
            {
                foreach (TextEditInputs input in Enum.GetValues(typeof(TextEditInputs)))
                {
                    if (Keyboard.KeyNowPressed(Translation.GetTextEditKey(input)))
                        return input;
                }
            }
            return TextEditInputs.NoInput;
        }

        public string GetPressedKeyString()
        {
            for (int number = 0; number < 10; number++)
            {
                if (Keyboard.KeyNowPressed(Keys.NumPad0 + number) || Keyboard.KeyNowPressed(Keys.D0 + number))
                    return number.ToString();
            }

            if (Keyboard.KeyNowPressed(Keys.OemPeriod))
                return ".";
            
            return "";
        }
    }
}
