using Microsoft.Xna.Framework;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using System.Net;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;
using System.Threading;

namespace PingPongClient.ControlLayer
{
    public class ConnectionControl : SubControlInterface
    {
        ConnectLobby ConnectionLobby { get; set; }

        LobbyVisualizer LobbyVisualizer { get { return Visualizer as LobbyVisualizer; } }

        bool Connecting { get; set; }

        public override GameMode GetMode { get { return GameMode.Lobby; } }

        public ConnectionControl(Control parent) : base(parent)
        {
            ConnectionLobby = new ConnectLobby();
            ConnectionLobby.ServerIP = "127.0.0.1";

            Input.AddPlayerInput(0, 0);

            Visualizer = new LobbyVisualizer(ConnectionLobby);

            Connecting = false;
        }

        public void SetStatus(string status)
        {
            ConnectionLobby.Status = status;
        }

        public override void HandleInput()
        {
            HandleTextInput();
        }

        public override void Update(GameTime gameTime)
        {

        }

        protected void HandleTextInput()
        {
            TextEditInputs editControl = Input.GetTextEditInput();

            if (editControl != TextEditInputs.NoInput)
            {
                switch (editControl)
                {
                    case TextEditInputs.Enter:
                        InitializeNetwork();
                        return;

                    case TextEditInputs.Delete:
                        if (ConnectionLobby.ServerIP.Length > 0)
                            ConnectionLobby.ServerIP = ConnectionLobby.ServerIP.Substring(0, ConnectionLobby.ServerIP.Length - 1);
                        return;
                }
            }
            else
            {
                ConnectionLobby.ServerIP += Input.GetNumberInput();
            }
        }

        protected void InitializeNetwork()
        {
            if (Connecting)
                return;

            ConnectionLobby.Status = "Connecting...";

            if (Network != null)
                return;

            IPAddress serverIP;
            if (!IPAddress.TryParse(ConnectionLobby.ServerIP, out serverIP))
            {
                ConnectionLobby.Status = "Invalid IP!";
                return;
            }

            InitializeNetworkHandler networkHandler = new InitializeNetworkHandler(serverIP, ParentControl.Logger);
            networkHandler.NetworkInitializingFinished += NetworkHandler_NetworkInitializingFinished;

            Connecting = true;

            new Thread(networkHandler.InitializeNetwork).Start();
        }

        private void NetworkHandler_NetworkInitializingFinished(InitializeNetworkHandler handler)
        {
            try
            {
                handler.NetworkInitializingFinished -= NetworkHandler_NetworkInitializingFinished;

                ConnectionLobby.Status = handler.Message;

                if (handler.Error)
                {
                    if (handler.Network != null)
                        handler.Network.Disconnect();
                }
                else
                {
                    Network = handler.Network;
                    Network.SessionDied += ParentControl.NetworkDeathHandler;
                    ParentControl.LobbyControl.SetServerIP(handler.ServerIP.ToString());
                    ParentControl.Mode = GameMode.Lobby;
                }
            }
            finally
            {
                Connecting = false;
            }
        }
    }
}
