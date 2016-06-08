using Microsoft.Xna.Framework.Input;
using GameLogicLibrary;

namespace PingPongClient.InputLayer.InputTranslators
{
    abstract class KeyboardControllTranslation
    {
        public abstract Keys GetKeyForAction(ClientControls action);
    }
}
