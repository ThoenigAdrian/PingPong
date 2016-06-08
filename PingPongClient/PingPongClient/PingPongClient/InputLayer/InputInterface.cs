using GameLogicLibrary;

namespace PingPongClient.InputLayer
{
    abstract class InputInterface
    {
        public abstract ClientControls GetInput();

        public abstract void Initialize();

        public abstract void Update();
    }
}
