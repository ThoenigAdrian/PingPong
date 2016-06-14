using Microsoft.Xna.Framework.Input;
using PingPongClient.InputLayer.KeyboardInputs;

namespace PingPongClient.InputLayer.InputTranslation
{
    class ControlTranslation
    {
        public Keys GetControlKey(ControlInputs action)
        {
            switch (action)
            {
                case ControlInputs.Pause:
                    return Keys.Space;
                case ControlInputs.Quit:
                    return Keys.Escape;
                case ControlInputs.Restart:
                    return Keys.Enter;
            }
            return Keys.None;
        }
    }
}
