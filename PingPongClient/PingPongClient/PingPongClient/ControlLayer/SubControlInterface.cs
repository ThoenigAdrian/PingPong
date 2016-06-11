using Microsoft.Xna.Framework;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using PingPongClient.VisualizeLayer;

namespace PingPongClient.ControlLayer
{
    public abstract class SubControlInterface
    {
        protected Control ParentControl { get; set; }
        protected VisualizerInterface Visualizer { get; set; }

        protected ClientNetwork Network
        {
            get { return ParentControl.Network; }
            set { ParentControl.Network = value; }
        }

        protected InputManager Input { get { return ParentControl.InputManager; } }

        public SubControlInterface(Control parent)
        {
            ParentControl = parent;
        }

        public abstract GameMode GetMode { get; }
        
        public abstract void Update(GameTime gameTime);

        public void Draw(GameTime gameTime)
        {
            Visualizer.Draw(gameTime);
        }

        public void InitializeVisualizer(XNAInitializationData initData)
        {
            Visualizer.Initialize(initData);
        }

        protected void Log(string text)
        {
            ParentControl.Log(text);
        }

    }
}
