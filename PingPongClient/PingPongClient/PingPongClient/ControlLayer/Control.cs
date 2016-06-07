using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Net;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using NetworkLibrary.Utility;
using NetworkLibrary;
using GameLogicLibrary;
using NetworkLibrary.DataStructs;
using PingPongClient.VisualizeLayer;

namespace PingPongClient
{
    public class Control : Game
    {
        ConnectionClient Connection { get; set; }
        GameVisualizerInterface Visualizer { get; set; }
        InputInterface Input = new KeyboardInput();

        public GraphicsDeviceManager GraphicManager { get; private set; }

        LogWriter Logger = new LogWriterConsole();

        public Control()
        {
            Visualizer = new XNAGameVisualizer();
            Visualizer.Initialize(this);
        }

        protected override void Initialize()
        {
            IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkConstants.SERVER_PORT);

            Input.Initialize();
            Connection = new ConnectionClient(server);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Visualizer.LoadContent();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();

            if(Input.GetInput() == ClientControls.Quit)
                this.Exit();

            ServerDataPackage data = Connection.GetServerData();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Visualizer.DrawGame();

            base.Draw(gameTime);
        }
    }
}
