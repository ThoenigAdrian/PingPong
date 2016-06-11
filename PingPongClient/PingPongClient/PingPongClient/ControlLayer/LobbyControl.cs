using Microsoft.Xna.Framework;
using NetworkLibrary;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using PingPongClient.VisualizeLayer;
using PingPongClient.VisualizeLayer.XNAVisualization;
using System.Net;
using System.Net.Sockets;

namespace PingPongClient.ControlLayer
{
    class LobbyControl : SubControlInterface
    {
        Lobby GameLobby { get; set; }

        LobbyVisualizer LobbyVisualizer { get { return base.Visualizer as LobbyVisualizer; } }

        public override GameMode GetMode { get { return GameMode.Lobby; } }

        public LobbyControl(Control parent) : base(parent)
        {
            GameLobby = new Lobby();
            GameLobby.ServerIP = "127.0.0.1";

            Input.AddPlayerInput(0, 0);

            Visualizer = new XNALobbyVisualizer();
            (Visualizer as LobbyVisualizer).SetLobby(GameLobby);
        }

        public override void Update(GameTime gameTime)
        {
            HandleTextInput();
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
                        if (GameLobby.ServerIP.Length > 0)
                            GameLobby.ServerIP = GameLobby.ServerIP.Substring(0, GameLobby.ServerIP.Length - 1);
                        return;
                }
            }
            else
            {
                GameLobby.ServerIP += Input.GetNumberInput();
            }
        }

        protected void InitializeNetwork()
        {
            GameLobby.Status = "";

            IPAddress serverIP;
            if (!IPAddress.TryParse(GameLobby.ServerIP, out serverIP))
            {
                GameLobby.Status = "Invalid IP!";
                return;
            }

            Log("Initializing network...");

            IPEndPoint server = new IPEndPoint(serverIP, NetworkConstants.SERVER_PORT);
            Socket connectionSocket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                if (Network != null)
                    Network.Disconnect();

                connectionSocket.Connect(server);
                Network = new ClientNetwork(connectionSocket, ParentControl.Logger);
                ParentControl.Mode = GameMode.Game; 
                return;
            }
            catch
            {
                Log("Could not establish connection!");
            }

            GameLobby.Status = "Could not establish connection!";
        }
    }
}
