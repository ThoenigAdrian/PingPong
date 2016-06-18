using Microsoft.Xna.Framework;
using PingPongClient.NetworkLayer;
using System.Net;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;
using System.Threading;
using PingPongClient.InputLayer.KeyboardInputs;

namespace PingPongClient.ControlLayer
{
    public class ConnectionControl : SubControlInterface
    {
        ConnectLobby ConnectionLobby { get; set; }

        LobbyVisualizer LobbyVisualizer { get { return Visualizer as LobbyVisualizer; } }

        public SessionConnectParameters Reconnect;

        bool Connecting { get; set; }

        public override GameMode GetMode { get { return GameMode.Connect; } }

        public ConnectionControl(Control parent) : base(parent)
        {
            ConnectionLobby = new ConnectLobby();
            ConnectionLobby.ServerIP = "127.0.0.1"; //213.47.183.165

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
            HandleSelectionInput();
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
                    case TextEditInputs.Delete:
                        if (ConnectionLobby.ServerIP.Length > 0)
                            ConnectionLobby.ServerIP = ConnectionLobby.ServerIP.Substring(0, ConnectionLobby.ServerIP.Length - 1);
                        return;
                }
            }
            else
            {
                ConnectionLobby.ServerIP += Input.GetNumberInputAsString();
            }
        }

        private void HandleSelectionInput()
        {
            SelectionInputs selectionInput = Input.GetSelectionInput();

            if (selectionInput != SelectionInputs.NoInput)
            {
                if (selectionInput == SelectionInputs.Select)
                {
                    InitializeNetwork(ConnectionLobby.ConnectOptions.Selection == 1);
                }
                else if (Reconnect != null)
                {
                    switch (selectionInput)
                    {
                        case SelectionInputs.Up:
                            ConnectionLobby.ConnectOptions.Selection--;
                            break;
                        case SelectionInputs.Down:
                            ConnectionLobby.ConnectOptions.Selection++;
                            break;
                    }
                }
            }
        }

        protected void InitializeNetwork(bool reconnect = false)
        {
            InitializeNetworkHandler networkHandler;

            lock (ConnectionLobby)
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

                SessionConnectParameters connectParams;

                if (reconnect)
                    connectParams = Reconnect;
                else
                    connectParams = new SessionConnectParameters(serverIP, ParentControl.NetworkDeathHandler);

                networkHandler = new InitializeNetworkHandler(connectParams, ParentControl.Logger);
                networkHandler.NetworkInitializingFinished += NetworkHandler_NetworkInitializingFinished;

                Connecting = true;
            }

            new Thread(networkHandler.InitializeNetwork).Start();
        }

        private void ShowReconnectOptions()
        {
            if (Reconnect != null)
                ConnectionLobby.SetReconnect(Reconnect.ServerIP.ToString(), Reconnect.SessionID.ToString());
            ConnectionLobby.ConnectOptions.Visible = true;
        }

        private void NetworkHandler_NetworkInitializingFinished(InitializeNetworkHandler handler)
        {
            try
            {
                handler.NetworkInitializingFinished -= NetworkHandler_NetworkInitializingFinished;

                ConnectionLobby.Status = handler.Message;

                if (!handler.Error)
                {
                    Network = handler.Network;

                    Reconnect = new SessionConnectParameters(handler.ConnectParameters.ServerIP, ParentControl.NetworkDeathHandler, Network.ClientSession);
                    ShowReconnectOptions();

                    ParentControl.OptionControl.SetServerIP(handler.ConnectParameters.ServerIP.ToString());
                    ParentControl.SwitchMode(GameMode.Lobby);
                }
            }
            finally
            {
                Connecting = false;
            }
        }
    }
}
