using Microsoft.Xna.Framework.Input;

namespace PingPongClient.InputLayer.InputTranslation
{
    class TextEditTranslation
    {
        public Keys GetTextEditKey(TextEditInputs input)
        {
            switch (input)
            {
                case TextEditInputs.Delete:
                    return Keys.Back;
                case TextEditInputs.Enter:
                    return Keys.Enter;
            }
            return Keys.None;
        }
    }
}
