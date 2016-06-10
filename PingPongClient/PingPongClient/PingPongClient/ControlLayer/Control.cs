using Microsoft.Xna.Framework;
using System.Net;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using NetworkLibrary.Utility;
using NetworkLibrary;
using GameLogicLibrary;
using PingPongClient.ControlLayer;
using GameLogicLibrary.GameObjects;
using PingPongClient.InputLayer.InputTranslation;
using System.Collections.Generic;
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
        GameMode Mode = GameMode.Lobby;

        GameStructure Structure { get; set; }

        ClientNetwork Network { get; set; }

        GraphicsDeviceManager GraphicsManager { get; set; }
        XNAVisualizeManager Visualizers { get; set; }
        Interpolation Interpolation;

        List<PlayerInput> ActivePlayers { get; set; }
        InputInterface ControlInput = new KeyboardInput(new ControlTranslation());

        public IPAddress ServerIP { get; set; }

        LogWriter Logger = new LogWriterConsole();

        public Control()
        {
            Structure = new GameStructure();
            ActivePlayers = new List<PlayerInput>();
            Interpolation = new Interpolation(Structure);
            Visualizers = new XNAVisualizeManager();
            GraphicsManager = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            ControlInput.Initialize();
            ActivePlayers.Add(new PlayerInput(new KeyboardInput(TranslationFactory.GetTranslationForPlayerIndex(0))));
            base.Initialize();
        }

        protected void InitializeNetwork()
        {
            if (ServerIP == null)
                return;

            Logger.Log("Initializing network...");

            IPEndPoint server = new IPEndPoint(ServerIP, NetworkConstants.SERVER_PORT);
            Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                if (Network != null)
                    Network.Disconnect();

                connectionSocket.Connect(server);
                Network = new ClientNetwork(connectionSocket);
            }
            catch
            {
                Logger.Log("Could not establish connection!");
            }
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
            ControlInput.Update(); // global keyboard update - dont call update on any other input!

            switch(Mode)
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
                            SendClientCommandos();
                            SendMovementInputs();
                            ApplyServerPositions();
                        }

                        HandleControlInputs();

                        Interpolation.Interpolate(gameTime);

                        break;
                    }
            }

            base.Update(gameTime);
        }

        protected void HandleTextInput()
        {
            //ControlInput.GetTextInput() == 
        }

        protected void HandleControlInputs()
        {
            //if (ControlInput.GetControlInput() != ClientControls.NoInput)
            //{
            //    ClientControlPackage controlPackage = new ClientControlPackage();
            //    controlPackage.ControlInput = ControlInput.GetControlInput();
            //    Network.SendClientControl(controlPackage);
            //}

            if (ControlInput.GetControlInput() == ClientControls.Pause)
            {
                InitializeNetwork();
            }

            if (ControlInput.GetControlInput() == ClientControls.Quit)
                this.Exit();
        }

        protected void SendMovementInputs()
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

        protected void SendClientCommandos()
        {
            if (ControlInput.GetControlInput() == ClientControls.Restart)
            {
                Network.SendUDPTestData(new PlayerMovementPackage());
            }
        }

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
