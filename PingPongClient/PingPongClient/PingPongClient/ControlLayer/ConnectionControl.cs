using Microsoft.Xna.Framework;
using NetworkLibrary;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using System.Net;
using System.Net.Sockets;
using System;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;

namespace PingPongClient.ControlLayer
{
    public class ConnectionControl : SubControlInterface
    {
        ConnectLobby ConnectionLobby { get; set; }

        LobbyVisualizer LobbyVisualizer { get { return base.Visualizer as LobbyVisualizer; } }

        public override GameMode GetMode { get { return GameMode.Lobby; } }

        public ConnectionControl(Control parent) : base(parent)
        {
            ConnectionLobby = new ConnectLobby();
            ConnectionLobby.ServerIP = "213.47.183.165";

            Input.AddPlayerInput(0, 0);

            Visualizer = new LobbyVisualizer(ConnectionLobby);
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
            ConnectionLobby.Status = "";

            IPAddress serverIP;
            if (!IPAddress.TryParse(ConnectionLobby.ServerIP, out serverIP))
            {
                ConnectionLobby.Status = "Invalid IP!";
                return;
            }

            Log("Initializing network...");

            IPEndPoint server = new IPEndPoint(serverIP, NetworkConstants.SERVER_PORT);
            Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                if (Network != null)
                    Network.Disconnect();

                IAsyncResult result = connectionSocket.BeginConnect(server, null, null);
                result.AsyncWaitHandle.WaitOne(5000, true);

                if (!connectionSocket.Connected)
                {
                    connectionSocket.Close();
                    ConnectionLobby.Status = "Connect timeout!";
                    return;
                }

                Network = new ClientNetwork(connectionSocket, ParentControl.Logger);
                Network.SessionDied += ParentControl.NetworkDeathHandler;
                ConnectionLobby.Status = "Connected.";
                ParentControl.LobbyControl.SetServerIP(ConnectionLobby.ServerIP);
                ParentControl.Mode = GameMode.Lobby;
                return;
            }
            catch (Exception ex)
            {
                ConnectionLobby.Status = "Connection failed!\n" + ex.Message;
            }
        }
    }
}
