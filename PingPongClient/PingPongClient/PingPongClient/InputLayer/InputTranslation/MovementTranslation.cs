using Microsoft.Xna.Framework.Input;
using PingPongClient.InputLayer.KeyboardInputs;

namespace PingPongClient.InputLayer.InputTranslation
{
    class MovementTranslation
    {
        Keys Up { get; set; }
        Keys Down { get; set; }

        public MovementTranslation(Keys up, Keys down)
        {
            Up = up;
            Down = down;
        }

        public Keys GetMovementKey(PlayerMovementInputs action)
        {
            switch (action)
            {
                case PlayerMovementInputs.Up:
                    return Up;
                case PlayerMovementInputs.Down:
                    return Down;
            }
            return Keys.None;
        }
    }
}
