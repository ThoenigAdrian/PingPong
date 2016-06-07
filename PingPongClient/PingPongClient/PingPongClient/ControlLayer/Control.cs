using Microsoft.Xna.Framework;
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

        public IPAddress ServerIP { get; set; }

        LogWriter Logger = new LogWriterConsole();

        public Control()
        {
            Structure = new GameStructure();
            Interpolation = new Interpolation(Structure);
            Visualizer = new XNAGameVisualizer(Structure);
            Visualizer.Initialize(this);
        }

        protected void ConnectToServer()
        {
            if (ServerIP == null)
                return;

            IPEndPoint server = new IPEndPoint(ServerIP, NetworkConstants.SERVER_PORT);
            Connection = new ClientConnection(server);
            Connection.Connect();
        }

        protected override void Initialize()
        {
            Input.Initialize();
            ConnectToServer();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Visualizer.ApplyResize();
            Visualizer.LoadContent();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();

            if (Connection != null)
            {
                if (Input.GetInput() != ClientControls.NoInput)
                {
                    ClientControlPackage controlPackage = new ClientControlPackage();
                    controlPackage.Input = Input.GetInput();
                    Connection.SendClientControl(controlPackage);
                }

                if (Input.GetInput() == ClientControls.Quit)
                {
                    Connection.Disconnect();
                    this.Exit();
                }

                ServerDataPackage data = Connection.GetServerData();

                if (data != null)
                    ApplyServerPositions(data);
            }
            else if (Input.GetInput() == ClientControls.Quit)
            {
                this.Exit();
            }

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
