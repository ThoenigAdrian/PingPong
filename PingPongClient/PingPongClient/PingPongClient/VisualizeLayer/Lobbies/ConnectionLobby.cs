using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Lobbies.SelectionLists;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class ConnectLobby : LobbyInterface
    {
        public string m_serverIP;
        public string ServerIP
        {
            get { return m_serverIP; }
            set
            {
                m_serverIP = value;
                if(NewSessionMode)
                    DrawableServerIP.Value = CreateIPString(value);
            }
        }

        string m_reconnectIP = "";
        string m_reconnectSession = "";

        public string Status
        {
            get { return DrawableStatus.Value; }
            set { DrawableStatus.Value = value; }
        }

        DrawableString DrawableServerIP;
        DrawableString DrawableStatus;
        public ConnectOptions ConnectOptions;

        bool NewSessionMode { get { return ConnectOptions.Selection == 0; } }

        public ConnectLobby()
        {
            DrawableServerIP = new DrawableString(CreateIPString(""), new Vector2(117, 200), Color.White);
            DrawableStatus = new DrawableString("", new Vector2(117, 250), Color.White);

            Strings.Add(DrawableServerIP);
            Strings.Add(DrawableStatus);

            ConnectOptions = new ConnectOptions();
            ConnectOptions.Visible = false;
            ConnectOptions.TopLeft = new Vector2(100, 100);
            ConnectOptions.SelectionChanged += AdjustTextToSelection;

            Lists.Add(ConnectOptions);
        }

        private void AdjustTextToSelection()
        {
            if (NewSessionMode)
                DrawableServerIP.Value = CreateIPString(m_serverIP);
            else
                DrawableServerIP.Value = CreateReconnectString(m_reconnectIP, m_reconnectSession);
        }

        public void SetReconnect(string ip, string session)
        {
            m_reconnectIP = ip;
            m_reconnectSession = session;

            AdjustTextToSelection();
        }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        string CreateIPString(string serverIP)
        {
            return "Enter server IP: " + serverIP;
        }

        string CreateReconnectString(string serverIP, string sessionID)
        {
            return "Reconnect to " + serverIP + ", Session ID: " + sessionID;
        }
    }
}
