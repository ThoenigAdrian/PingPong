using Microsoft.Xna.Framework.Input;

namespace PingPongClient.InputLayer.InputTranslation
{
    class TranslationFactory
    {
        public static MovementTranslation GetTranslationForPlayerIndex(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0:
                    return new MovementTranslation(Keys.Up, Keys.Down);
                case 1:
                    return new MovementTranslation(Keys.W, Keys.S);
                case 2:
                    return new MovementTranslation(Keys.NumPad8, Keys.NumPad2);
            }

            return null;
        }
    }
}
