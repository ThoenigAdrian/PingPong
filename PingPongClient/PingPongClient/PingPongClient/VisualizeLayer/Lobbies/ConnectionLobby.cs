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
            DrawableServerIP = CreateServerIPString();
            DrawableStatus = CreateStatusString();
        }

        public override DrawableString[] GetDrawableStrings
        {
            get
            {
                DrawableString[] drawableStrings = new DrawableString[2];
                drawableStrings[0] = DrawableServerIP;
                drawableStrings[1] = DrawableStatus;
                return drawableStrings;
            }
        }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        string CreateIPString(string serverIP)
        {
            return "Enter server IP: " + serverIP;
        }

        DrawableString CreateServerIPString()
        {
            DrawableString serverIPDraw = new DrawableString(CreateIPString(""));
            serverIPDraw.StringColor = Color.White;
            serverIPDraw.PosX = 100;
            serverIPDraw.PosY = 100;

            return serverIPDraw;
        }

        DrawableString CreateStatusString()
        {
            DrawableString statusDraw = new DrawableString("");
            statusDraw.StringColor = Color.White;
            statusDraw.PosX = 100;
            statusDraw.PosY = 150;

            return statusDraw;
        }
    }
}
