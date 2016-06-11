using Microsoft.Xna.Framework;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using NetworkLibrary.Utility;
using PingPongClient.ControlLayer;
using System;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer;

namespace PingPongClient
{
    public enum GameMode
    {
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

        LobbyControl LobbyControl { get; set; }
        GameControl GameControl { get; set; }

        public ClientNetwork Network { get; set; }
        public InputManager InputManager { get; set; }

        GraphicsDeviceManager GraphicsManager { get; set; }

        public LogWriter Logger = new LogWriterConsole();

        public Control()
        {
            LobbyControl = new LobbyControl(this);
            GameControl = new GameControl(this);

            InputManager = new InputManager();
            GraphicsManager = new GraphicsDeviceManager(this);

            ActiveControl = LobbyControl;
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

            LobbyControl.InitializeVisualizer(initData);
            GameControl.InitializeVisualizer(initData);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            ActiveControl.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ActiveControl.Draw(gameTime);

            base.Draw(gameTime);
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
