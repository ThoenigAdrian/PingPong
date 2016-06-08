using System;
using GameLogicLibrary;
using Microsoft.Xna.Framework.Input;

namespace PingPongClient.InputLayer.InputTranslation
{
    class MovementTranslation : KeyboardControlTranslation
    {
        Keys Up { get; set; }
        Keys Down { get; set; }

        public MovementTranslation(Keys up, Keys down)
        {
            Up = up;
            Down = down;
        }

        public override Keys GetMovementKey(ClientMovement action)
        {
            switch (action)
            {
                case ClientMovement.Up:
                    return Up;
                case ClientMovement.Down:
                    return Down;
            }
            return Keys.None;
        }

        public override Keys GetControlKey(ClientControls control)
        {
            return Keys.None;
        }
    }
}
