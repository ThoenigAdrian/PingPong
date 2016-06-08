using GameLogicLibrary;

namespace PingPongClient.InputLayer
{
    abstract class InputInterface
    {
        public abstract ClientMovement GetMovementInput();

        public abstract ClientControls GetControlInput();

        public abstract void Initialize();

        public abstract void Update();
    }
}
