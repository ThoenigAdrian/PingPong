using InputFunctionality.KeyboardAdapter;

namespace PingPongClient.InputLayer
{
    abstract class InputInterface
    {
    }

    abstract class InputInterfaceKeyboard
    {
        protected KeyboardAdvanced Keyboard { get; private set; }

        protected InputInterfaceKeyboard(KeyboardAdvanced keyboard)
        {
            Keyboard = keyboard;
        }
    }
}
