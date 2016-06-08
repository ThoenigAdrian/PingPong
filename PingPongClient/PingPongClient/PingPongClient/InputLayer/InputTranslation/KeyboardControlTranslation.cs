using Microsoft.Xna.Framework.Input;
using GameLogicLibrary;

namespace PingPongClient.InputLayer.InputTranslation
{
    abstract class KeyboardControlTranslation
    {
        public abstract Keys GetMovementKey(ClientMovement movement);

        public abstract Keys GetControlKey(ClientControls control);
    }
}
