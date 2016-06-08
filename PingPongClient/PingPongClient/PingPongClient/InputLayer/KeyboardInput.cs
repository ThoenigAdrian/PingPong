using InputFunctionality.KeyboardAdapter;
using GameLogicLibrary;
using Microsoft.Xna.Framework.Input;

namespace PingPongClient.InputLayer
{
    class KeyboardInput : InputInterface
    {
        KeyboardAdvanced KeyboardAdapter = new KeyboardAdvanced();

        public KeyboardInput()
        {
            
        }

        public override void Initialize()
        {
            KeyboardAdapter.Initialize();
        }

        public override void Update()
        {
            KeyboardAdapter.UpdateState();
        }

        public override ClientControls GetInput()
        {
            if (KeyboardAdapter.KeyNowPressed(Keys.Up))
                return ClientControls.Up;

            if (KeyboardAdapter.KeyNowPressed(Keys.Down))
                return ClientControls.Down;

            if (KeyboardAdapter.KeyNowPressed(Keys.Space))
                return ClientControls.Pause;

            if (KeyboardAdapter.KeyNowPressed(Keys.Escape))
                return ClientControls.Quit;

            if (KeyboardAdapter.KeyNowPressed(Keys.Enter))
                return ClientControls.Restart;

            if (KeyboardAdapter.KeyNowReleased(Keys.Up) || KeyboardAdapter.KeyNowReleased(Keys.Down))
                return ClientControls.StopMoving;

            return ClientControls.NoInput;
        }
    }
}
