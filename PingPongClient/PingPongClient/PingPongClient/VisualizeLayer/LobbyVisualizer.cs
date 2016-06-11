using Microsoft.Xna.Framework;
using PingPongClient.ControlLayer;

namespace PingPongClient.VisualizeLayer
{
    abstract class LobbyVisualizer : VisualizerInterface
    {
        Lobby GameLobby;

        public void SetLobby(Lobby lobby)
        {
            GameLobby = lobby;
        }

        public override void Draw(GameTime gameTime)
        {
            if (GameLobby == null || !CanDraw())
                return;

            DrawBegin();

            DrawString(CreateServerIPString());
            DrawString(CreateStatusString());

            DrawEnd();
        }

        protected abstract void DrawBegin();

        protected abstract void DrawString(DrawableString drawString);

        protected abstract void DrawEnd();

        protected abstract bool CanDraw();

        DrawableString CreateServerIPString()
        {
            DrawableString serverIPDraw = new DrawableString("Enter server IP: " + GameLobby.ServerIP);
            serverIPDraw.PosX = 100;
            serverIPDraw.PosY = 100;

            return serverIPDraw;
        }

        DrawableString CreateStatusString()
        {
            DrawableString statusDraw = new DrawableString(GameLobby.Status);
            statusDraw.PosX = 100;
            statusDraw.PosY = 150;

            return statusDraw;
        }
    }

    class DrawableString
    {
        public string Value;
        public float PosX = 0;
        public float PosY = 0;

        public DrawableString(string value)
        {
            Value = value;
        }
    }
}
