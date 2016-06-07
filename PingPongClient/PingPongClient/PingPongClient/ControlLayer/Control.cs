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
using PingPongClient.ControlLayer;
using GameLogicLibrary.GameObjects;

namespace PingPongClient
{
    public class Control : Game
    {
        public GameStructure Structure { get; set; }

        ClientConnection Connection { get; set; }
        GameVisualizerInterface Visualizer { get; set; }
        InputInterface Input = new KeyboardInput();
        Interpolation Interpolation;

        LogWriter Logger = new LogWriterConsole();

        public Control()
        {
            Structure = new GameStructure();
            Interpolation = new Interpolation(Structure);
            Visualizer = new XNAGameVisualizer(Structure);
            Visualizer.Initialize(this);
        }

        protected override void Initialize()
        {
            IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkConstants.SERVER_PORT);

            Input.Initialize();
            Connection = new ClientConnection(server);
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
            ApplyServerPositions(data);
            Interpolation.Interpolate(gameTime);

            base.Update(gameTime);
        }

        protected void ApplyServerPositions(ServerDataPackage data)
        {
            Structure.m_player1.PosX = data.Player1PosX;
            Structure.m_player1.PosY = data.Player1PosY;

            Structure.m_player2.PosX = data.Player2PosX;
            Structure.m_player2.PosY = data.Player2PosY;

            Structure.m_ball.PosX = data.BallPosX;
            Structure.m_ball.PosY = data.BallPosY;
        }

        protected override void Draw(GameTime gameTime)
        {
            Visualizer.DrawGame();

            base.Draw(gameTime);
        }
    }
}
