using Microsoft.Xna.Framework;
using System.Net;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using NetworkLibrary.Utility;
using NetworkLibrary;
using GameLogicLibrary;
using PingPongClient.ControlLayer;
using GameLogicLibrary.GameObjects;
using System;
using System.Net.Sockets;
using NetworkLibrary.DataPackages;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer.XNAVisualization;

namespace PingPongClient
{
    enum GameMode
    {
        Lobby,
        Game
    }

    public class Control : Game
    {
        GameMode m_mode = GameMode.Lobby;
        GameMode Mode
        {
            get { return m_mode; }
            set
            {
                m_mode = value;
                Visualizers.CurrentMode = value;
            }
        }

        GameStructure Structure { get; set; }

        Lobby GameLobby { get; set; }

        ClientNetwork Network { get; set; }

        GraphicsDeviceManager GraphicsManager { get; set; }
        XNAVisualizeManager Visualizers { get; set; }
        Interpolation Interpolation;

        InputManager InputManager { get; set; }

        public IPAddress ServerIP { get; set; }

        LogWriter Logger = new LogWriterConsole();

        public Control()
        {
            Structure = new GameStructure();
            GameLobby = new Lobby();
            InputManager = new InputManager();
            Interpolation = new Interpolation(Structure);
            Visualizers = new XNAVisualizeManager();
            GraphicsManager = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            GameLobby.ServerIP = ServerIP.ToString();
            Visualizers.SetLobby(GameLobby);
            InputManager.Initialize();
            base.Initialize();
        }

        protected void InitializeNetwork()
        {
            GameLobby.Status = "";

            IPAddress serverIP;
            if (!IPAddress.TryParse(GameLobby.ServerIP, out serverIP))
            {
                GameLobby.Status = "Invalid IP!";
            }

            Logger.Log("Initializing network...");

            IPEndPoint server = new IPEndPoint(serverIP, NetworkConstants.SERVER_PORT);
            Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                if (Network != null)
                    Network.Disconnect();

                connectionSocket.Connect(server);
                Network = new ClientNetwork(connectionSocket);
                Mode = GameMode.Game;
                return;
            }
            catch
            {
                Logger.Log("Could not establish connection!");
            }

            GameLobby.Status = "Could not establish connection!";
        }


        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            XNAInitializationData initData = new XNAInitializationData();
            initData.Content = Content;
            initData.GraphicManager = GraphicsManager;
            initData.SpriteBatch = new SpriteBatch(GraphicsManager.GraphicsDevice);

            Visualizers.InitializeData = initData;

            Visualizers.SetGameStructure(Structure);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            HandleControlInputs();

            switch (Mode)
            {
                case GameMode.Lobby:
                    {
                        HandleTextInput();
                        break;
                    }

                case GameMode.Game:
                    {
                        if (Network != null)
                        {
                            //SendClientCommandos();
                            SendMovementInputs();
                            ApplyServerPositions();
                        }

                        Interpolation.Interpolate(gameTime);

                        break;
                    }
            }

            base.Update(gameTime);
        }

        protected void HandleTextInput()
        {
            TextEditInputs editControl = InputManager.GetTextEditInput();

            if (editControl != TextEditInputs.NoInput)
            {
                switch (editControl)
                {
                    case TextEditInputs.Enter:
                        InitializeNetwork();
                        return;

                    case TextEditInputs.Delete:
                        if(GameLobby.ServerIP.Length > 0)
                            GameLobby.ServerIP = GameLobby.ServerIP.Substring(0, GameLobby.ServerIP.Length - 1);
                        return;
                }
            }
            else
            {
                GameLobby.ServerIP += InputManager.GetNumberInput();
            }
        }

        protected void HandleControlInputs()
        {
            //if (ControlInput.GetControlInput() != ClientControls.NoInput)
            //{
            //    ClientControlPackage controlPackage = new ClientControlPackage();
            //    controlPackage.ControlInput = ControlInput.GetControlInput();
            //    Network.SendClientControl(controlPackage);
            //}

            if (InputManager.GetControlInput() == ControlInputs.Quit)
                this.Exit();
        }

        protected void SendMovementInputs()
        {
            PlayerInputs[] playerInputs = InputManager.GetMovementInput();

            foreach (PlayerInputs inputs in playerInputs)
            {
                if (inputs.MovementInput != PlayerMovementInputs.NoInput)
                {
                    PlayerMovementPackage movementPackage = new PlayerMovementPackage();
                    movementPackage.PlayerID = inputs.ID;

                    switch (inputs.MovementInput)
                    {
                        case PlayerMovementInputs.Up:
                            movementPackage.PlayerMovement = ClientMovement.Up;
                            break;

                        case PlayerMovementInputs.Down:
                            movementPackage.PlayerMovement = ClientMovement.Down;
                            break;

                        case PlayerMovementInputs.StopMoving:
                            movementPackage.PlayerMovement = ClientMovement.StopMoving;
                            break;
                    }

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

        //protected void SendClientCommandos()
        //{
        //    if (ControlInput.GetControlInput() == ClientControls.Restart)
        //    {
        //        Network.SendUDPTestData(new PlayerMovementPackage());
        //    }
        //}

        protected override void Draw(GameTime gameTime)
        {
            Visualizers.Draw();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (Network != null)
                Network.Disconnect();
        }
    }
}
