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
using System.Collections.Generic;

namespace PingPongClient
{
    public enum GameMode
    {
        Connect,
        Options,
        Registration,
        Status,
        Game,
        Finish
    }

    public class Control : Game
    {
        public GameMode Mode
        {
            get { return ActiveControl.GetMode; }
            private set
            {
                if (value != Mode)
                {
                    ActiveControl = GetSubControl(value);
                    ActiveControl.OnEnter();
                }
            }
        }
        
        Dictionary<GameMode, SubControlInterface> SubControls { get; set; }
        SubControlInterface ActiveControl { get; set; }

        public ConnectionControl ConnectionControl { get; set; }
        public GameOptionsControl OptionControl { get; set; }
        public PlayerRegistrationControl RegistrationControl { get; set; }
        public MatchmakingStatusControl StatusControl { get; set; }
        public GameControl GameControl { get; set; }
        public FinishControl FinishControl { get; set; }

        public ClientNetwork Network { get; set; }
        public InputManager InputManager { get; set; }

        GraphicsDeviceManager GraphicsManager { get; set; }

        public GameLogger Logger = new LogWriterConsole();

        volatile bool m_networkDied;

        public Control()
        {
            InputManager = new InputManager();
            GraphicsManager = new GraphicsDeviceManager(this);

            SubControls = new Dictionary<GameMode, SubControlInterface>();

            ConnectionControl = new ConnectionControl(this);
            OptionControl = new GameOptionsControl(this);
            RegistrationControl = new PlayerRegistrationControl(this);
            StatusControl = new MatchmakingStatusControl(this);
            GameControl = new GameControl(this);
            FinishControl = new FinishControl(this);

            SubControls.Add(GameMode.Connect, ConnectionControl);
            SubControls.Add(GameMode.Options, OptionControl);
            SubControls.Add(GameMode.Registration, RegistrationControl);
            SubControls.Add(GameMode.Status, StatusControl);
            SubControls.Add(GameMode.Game, GameControl);
            SubControls.Add(GameMode.Finish, FinishControl);
            
            ActiveControl = GetSubControl(GameMode.Connect);

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

            foreach (SubControlInterface subControl in SubControls.Values)
            {
                subControl.InitializeVisualizer(initData);
            }

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (m_networkDied)
            {
                ConnectionControl.SetStatus("Connection died.");
                CleanUpNetwork();
                SwitchMode(GameMode.Connect);
            }

            InputManager.Update();

            if (IsActive)
            {
                if (IsExitInput())
                    HandleExitInput();

                ActiveControl.HandleInput();
            }

            if (Network != null)
            {
                foreach (PackageInterface package in Network.GetTCPPackages())
                    ActiveControl.ProcessServerData(package);
            }

            // a switch to finish can happen in between those lines which disconnects -> check again if its dead
            if (Network != null)
            { 
                PackageInterface udpPackage = Network.GetUDPPackage();
                if(udpPackage != null)
                    ActiveControl.ProcessServerData(udpPackage);
            }

            ActiveControl.Update(gameTime);

            base.Update(gameTime);
        }

        private bool IsExitInput()
        {
            return InputManager.GetControlInput() == ControlInputs.Quit;
        }

        protected void HandleExitInput()
        {
            if (Mode == GameMode.Connect)
            {
                Exit();
            }
            else
            {
                Disconnect();
                SwitchMode(GameMode.Connect);
            }
        }

        public SubControlInterface GetSubControl(GameMode mode)
        {
            SubControlInterface subControl;
            if (!SubControls.TryGetValue(mode, out subControl))
                throw new Exception("SubControl not found!");

            return subControl;
        }

        public void SwitchMode(GameMode mode)
        {
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

        public void Disconnect()
        {
            if (Network != null)
            {
                Network.SessionDied -= NetworkDeathHandler;
                Network.Disconnect();
                ConnectionControl.SetStatus("Disconnected.");
            }

            CleanUpNetwork();
        }

        void CleanUpNetwork()
        {
            Network = null;
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
