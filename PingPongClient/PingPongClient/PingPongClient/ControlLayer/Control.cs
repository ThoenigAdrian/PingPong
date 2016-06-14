using Microsoft.Xna.Framework;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using NetworkLibrary.Utility;
using PingPongClient.ControlLayer;
using System;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer.Visualizers;
using NetworkLibrary.NetworkImplementations;

namespace PingPongClient
{
    public enum GameMode
    {
        Connect,
        Lobby,
        Game
    }

    public class Control : Game
    {
        public GameMode Mode
        {
            get { return ActiveControl.GetMode; }
            set
            {
                switch(value)
                {
                    case GameMode.Connect:
                        ActiveControl = ConnectionControl;
                        break;
                    case GameMode.Lobby:
                        ActiveControl = LobbyControl;
                        break;
                    case GameMode.Game:
                        ActiveControl = GameControl;
                        break;
                }
            }
        }

        SubControlInterface ActiveControl { get; set; }

        public ConnectionControl ConnectionControl { get; set; }
        public LobbyControl LobbyControl { get; set; }
        public GameControl GameControl { get; set; }

        public ClientNetwork Network { get; set; }
        public InputManager InputManager { get; set; }

        GraphicsDeviceManager GraphicsManager { get; set; }

        public LogWriter Logger = new LogWriterConsole();

        public Control()
        {
            InputManager = new InputManager();
            GraphicsManager = new GraphicsDeviceManager(this);

            ConnectionControl = new ConnectionControl(this);
            LobbyControl = new LobbyControl(this);
            GameControl = new GameControl(this);

            ActiveControl = ConnectionControl;
        }

        protected override void Initialize()
        {
            InputManager.Initialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            XNAInitializationData initData = new XNAInitializationData();
            initData.Content = Content;
            initData.GraphicManager = GraphicsManager;
            initData.SpriteBatch = new SpriteBatch(GraphicsManager.GraphicsDevice);

            ConnectionControl.InitializeVisualizer(initData);
            LobbyControl.InitializeVisualizer(initData);
            GameControl.InitializeVisualizer(initData);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            ActiveControl.Update(gameTime);

            if (IsActive)
            {
                HandleControlInputs();
                ActiveControl.HandleInput();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ActiveControl.Draw(gameTime);

            base.Draw(gameTime);
        }

        public void NetworkDeathHandler(NetworkInterface sender, int sessionID)
        {
            Network.SessionDied -= NetworkDeathHandler;
            Network.Disconnect();
            Network = null;
            ConnectionControl.SetStatus("Connection died.");
            Mode = GameMode.Connect;
        }

        protected void HandleControlInputs()
        {
            if (InputManager.GetControlInput() == ControlInputs.Quit)
                Exit();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (Network != null)
                Network.Disconnect();
        }

        public void Log(string text)
        {
            if (Logger != null)
                Logger.Log(text);
        }
    }
}
