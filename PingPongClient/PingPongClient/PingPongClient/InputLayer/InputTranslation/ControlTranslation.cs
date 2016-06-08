using System;
using GameLogicLibrary;
using Microsoft.Xna.Framework.Input;

namespace PingPongClient.InputLayer.InputTranslation
{
    class ControlTranslation : KeyboardControlTranslation
    {
        public override Keys GetControlKey(ClientControls action)
        {
            switch (action)
            {
                case ClientControls.Pause:
                    return Keys.Space;
                case ClientControls.Quit:
                    return Keys.Escape;
                case ClientControls.Restart:
                    return Keys.Enter;
            }
            return Keys.None;
        }

        public override Keys GetMovementKey(ClientMovement action)
        {
            return Keys.None;
        }
    }
}
