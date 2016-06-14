using InputFunctionality.KeyboardAdapter;
using Microsoft.Xna.Framework.Input;
using PingPongClient.InputLayer.InputTranslation;
using System;

namespace PingPongClient.InputLayer.KeyboardInputs
{
    public enum TextEditInputs
    {
        Number,
        Delete,
        Enter,
        NoInput
    }

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
            int keyValue = GetPressedKeyValue();

            if (keyValue > -1)
                return keyValue.ToString();

            if (Keyboard.KeyNowPressed(Keys.OemPeriod))
                return ".";
            
            return "";
        }

        public int GetPressedKeyValue()
        {
            for (int number = 0; number < 10; number++)
            {
                if (Keyboard.KeyNowPressed(Keys.NumPad0 + number) || Keyboard.KeyNowPressed(Keys.D0 + number))
                    return number;
            }

            return -1;
        }
    }
}
