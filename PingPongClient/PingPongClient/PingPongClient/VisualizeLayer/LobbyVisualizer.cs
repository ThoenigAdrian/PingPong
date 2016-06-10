using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer
{
    abstract class LobbyVisualizer
    {
        Lobby GameLobby;

        public LobbyVisualizer()
        {
            GameLobby = new Lobby();
        }

        public void SetLobby(Lobby lobby)
        {
            GameLobby = lobby;
        }

        public void DrawLobby()
        {
            if (GameLobby == null || !CanDraw())
                return;

            DrawBegin();

            DrawString(GameLobby.ServerIP);

            DrawEnd();
        }

        protected abstract void DrawBegin();

        protected abstract void DrawString(DrawableString drawString);

        protected abstract void DrawEnd();

        protected abstract bool CanDraw();
    }

    class Lobby
    {
        public DrawableString ServerIP = new DrawableString("ServerIP");
    }

    class DrawableString
    {
        public string Value;
        public float PosX;
        public float PosY;
        public float Height;
        public float Width;

        public DrawableString(string value)
        {
            Value = value;
        }
    }
}
