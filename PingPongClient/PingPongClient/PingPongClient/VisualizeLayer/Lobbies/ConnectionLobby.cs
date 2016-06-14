using Microsoft.Xna.Framework;

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
                DrawableServerIP.Value = CreateIPString(value);
            }
        }

        public string Status
        {
            get { return DrawableStatus.Value; }
            set { DrawableStatus.Value = value; }
        }

        DrawableString DrawableServerIP;
        DrawableString DrawableStatus;

        public ConnectLobby()
        {
            DrawableServerIP = new DrawableString(CreateIPString(""), new Vector2(100, 100), Color.White);
            DrawableStatus = new DrawableString("", new Vector2(100, 150), Color.White);

            Strings.Add(DrawableServerIP);
            Strings.Add(DrawableStatus);
        }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        string CreateIPString(string serverIP)
        {
            return "Enter server IP: " + serverIP;
        }
    }
}
