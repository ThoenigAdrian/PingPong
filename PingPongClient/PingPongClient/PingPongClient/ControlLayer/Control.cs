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
using PingPongClient.InputLayer.InputTranslation;
using System.Collections.Generic;

namespace PingPongClient
{
    public class Control : Game
    {
        GameStructure Structure { get; set; }

        ClientNetwork Network { get; set; }
        GameVisualizerInterface Visualizer { get; set; }
        List<PlayerInput> ActivePlayers { get; set; }
        InputInterface ControlInput = new KeyboardInput(new ControlTranslation());
        Interpolation Interpolation;

        public IPAddress ServerIP { get; set; }

        LogWriter Logger = new LogWriterConsole();

        public Control()
        {
            Structure = new GameStructure();
            ActivePlayers = new List<PlayerInput>();
            Interpolation = new Interpolation(Structure);
            Visualizer = new XNAGameVisualizer(Structure);
            Visualizer.Initialize(this);
        }

        protected void ConnectToServer()
        {
            if (ServerIP == null)
                return;

            IPEndPoint server = new IPEndPoint(ServerIP, NetworkConstants.SERVER_PORT);

            try
            {
                Network = new ClientNetwork(server);
            }
            catch
            {
                Logger.Log("Could not establish connection!");
            }
        }

        protected override void Initialize()
        {
            ControlInput.Initialize();
            ConnectToServer();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Visualizer.LoadContent();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            ControlInput.Update(); // global keyboard update - dont call update on any other input!

            if (Network != null)
            {
                ApplyMovementInputs();
                ApplyControlInputs();
            }

            else if (ControlInput.GetControlInput() == ClientControls.Quit)
            {
                this.Exit();
            }

            Interpolation.Interpolate(gameTime);

            base.Update(gameTime);
        }

        protected void ApplyControlInputs()
        {
            if (ControlInput.GetControlInput() != ClientControls.NoInput)
            {
                ClientControlPackage controlPackage = new ClientControlPackage();
                controlPackage.ControlInput = ControlInput.GetControlInput();
                Network.SendClientControl(controlPackage);
            }

            if (ControlInput.GetControlInput() == ClientControls.Quit)
            {
                Network.Disconnect();
                this.Exit();
            }
        }

        protected void ApplyMovementInputs()
        {
            foreach (PlayerInput player in ActivePlayers)
            {
                ClientMovement playerInput = player.Input.GetMovementInput();

                if (playerInput != ClientMovement.NoInput)
                {
                    PlayerMovementPackage movementPackage = new PlayerMovementPackage();
                    movementPackage.PlayerID = player.ID;
                    movementPackage.PlayerMovement = playerInput;

                    Network.SendPlayerMovement(movementPackage);
                }
            }
        }

        protected void ApplyServerPositions()
        {
            ServerDataPackage data = Network.GetServerData();

            if (data == null)
                return;

            for (int i = 0; i < Structure.m_players.Count; i++)
            {
                
            }

            Structure.m_players[0].PosX = data.Player1PosX;
            Structure.m_players[0].PosY = data.Player1PosY;

            Structure.m_players[1].PosX = data.Player2PosX;
            Structure.m_players[1].PosY = data.Player2PosY;

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
