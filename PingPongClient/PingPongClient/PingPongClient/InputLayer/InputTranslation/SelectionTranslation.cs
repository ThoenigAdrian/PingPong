using Microsoft.Xna.Framework.Input;
using PingPongClient.InputLayer.KeyboardInputs;

namespace PingPongClient.InputLayer.InputTranslation
{
    class SelectionTranslation
    {
        public Keys GetSelectionKey(SelectionInputs selection)
        {
            switch (selection)
            {
                case SelectionInputs.Select:
                    return Keys.Enter;
                case SelectionInputs.Up:
                    return Keys.Up;
                case SelectionInputs.Down:
                    return Keys.Down;
                case SelectionInputs.Left:
                    return Keys.Left;
                case SelectionInputs.Right:
                    return Keys.Right;
            }
            return Keys.None;
        }
    }
}
