using GameLogicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingPongClient.InputLayer
{
    abstract class InputInterface
    {
        public abstract ClientControls GetInput();

        public abstract void Initialize();

        public abstract void Update();
    }
}
