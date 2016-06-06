using InputFunctionality.KeyboardAdapter;
using GameLogicLibrary;
using Microsoft.Xna.Framework.Input;
using System;

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

        public override ClientInput GetInput()
        {
            if (KeyboardAdapter.KeyNowPressed(Keys.Up))
                return ClientInput.Up;

            if (KeyboardAdapter.KeyNowPressed(Keys.Down))
                return ClientInput.Down;

            if (KeyboardAdapter.KeyNowPressed(Keys.Space))
                return ClientInput.Pause;

            if (KeyboardAdapter.KeyNowPressed(Keys.Escape))
                return ClientInput.Quit;

            if (KeyboardAdapter.KeyNowPressed(Keys.Enter))
                return ClientInput.Restart;

            if (KeyboardAdapter.KeyNowReleased(Keys.Up) || KeyboardAdapter.KeyNowReleased(Keys.Down))
                return ClientInput.StopMoving;

            return ClientInput.NoInput;
        }
    }
}
