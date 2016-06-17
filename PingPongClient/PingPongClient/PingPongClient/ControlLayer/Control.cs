using Microsoft.Xna.Framework;
using PingPongClient.InputLayer;
using PingPongClient.InputLayer.KeyboardInputs;
using PingPongClient.NetworkLayer;
using NetworkLibrary.Utility;
using PingPongClient.ControlLayer;
using System;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer.Visualizers;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.DataPackages;

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
            private set
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

        private ResponseRequest CurrentResponseRequest { get; set; }

        public ConnectionControl ConnectionControl { get; set; }
        public LobbyControl LobbyControl { get; set; }
        public GameControl GameControl { get; set; }

        public ClientNetwork Network { get; set; }
        public InputManager InputManager { get; set; }

        GraphicsDeviceManager GraphicsManager { get; set; }

        public LogWriter Logger = new LogWriterConsole();

        volatile bool m_networkDied;

        public Control()
        {
            InputManager = new InputManager();
            GraphicsManager = new GraphicsDeviceManager(this);

            ConnectionControl = new ConnectionControl(this);
            LobbyControl = new LobbyControl(this);
            GameControl = new GameControl(this);

            ActiveControl = ConnectionControl;

            m_networkDied = false;
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
            if (m_networkDied)
                CleanUpNetwork();

            InputManager.Update();

            DetectExitInput();

            CheckResponse();

            ActiveControl.Update(gameTime);

            if (IsActive && CurrentResponseRequest == null)
            {
                ActiveControl.HandleInput();
            }

            base.Update(gameTime);
        }

        protected void DetectExitInput()
        {
            if (InputManager.GetControlInput() == ControlInputs.Quit)
                Exit();
        }

        public bool IssueServerResponse(ResponseRequest responseRequest)
        {
            if (Network == null || CurrentResponseRequest != null)
                return false;

            CurrentResponseRequest = responseRequest;
            Network.IssueServerResponse(responseRequest);
            return true;
        }

        private void CheckResponse()
        {
            if(CurrentResponseRequest != null && CurrentResponseRequest.State != ResponseRequest.ResponseState.Pending)
            {
                if (CurrentResponseRequest.State == ResponseRequest.ResponseState.Received)
                {
                    PackageInterface package = CurrentResponseRequest.ResponsePackage;
                    CurrentResponseRequest = null;
                    ActiveControl.ProcessServerResponse(package);
                }
                else
                {
                    PackageType type = CurrentResponseRequest.ResponsePackageType;
                    CurrentResponseRequest = null;
                    ActiveControl.HandleResponseTimeout(type);
                }
            }
        }

        public void SwitchMode(GameMode mode)
        {
            if (CurrentResponseRequest != null)
                throw new Exception("Mode switch while waiting for a response!");

            Mode = mode;
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
            m_networkDied = true;
        }

        void CleanUpNetwork()
        {
            Network = null;
            CurrentResponseRequest = null;
            ConnectionControl.SetStatus("Connection died.");
            Mode = GameMode.Connect;
            m_networkDied = false;
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
