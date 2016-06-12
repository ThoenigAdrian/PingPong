using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class RequestLobby : LobbyInterface
    {
        public string Status
        {
            get { return DrawableStatus.Value; }
            set { DrawableStatus.Value = value; }
        }

        DrawableString DrawableStatus;
        DrawableString DrawableInfo;

        public RequestLobby()
        {
            DrawableStatus = CreateStatusString();
            DrawableInfo = CreateInfoString();
        }

        public override DrawableString[] GetDrawableStrings
        {
            get
            {
                DrawableString[] drawableStrings = new DrawableString[2];
                drawableStrings[0] = DrawableStatus;
                drawableStrings[1] = DrawableInfo;
                return drawableStrings;
            }
        }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        DrawableString CreateStatusString()
        {
            DrawableString statusDraw = new DrawableString("");
            statusDraw.StringColor = Color.White;
            statusDraw.PosX = 100;
            statusDraw.PosY = 100;

            return statusDraw;
        }

        DrawableString CreateInfoString()
        {
            DrawableString statusDraw = new DrawableString("Press Enter to start a new game.\nPress Space to join an exisiting game.");
            statusDraw.StringColor = Color.White;
            statusDraw.PosX = 100;
            statusDraw.PosY = 150;

            return statusDraw;
        }

    }
}
