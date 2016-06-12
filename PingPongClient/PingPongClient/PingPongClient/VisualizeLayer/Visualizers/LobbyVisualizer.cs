using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer.Lobbies;

namespace PingPongClient.VisualizeLayer.Visualizers
{
    class LobbyVisualizer : VisualizerInterface
    {
        LobbyInterface DrawableLobby;

        public LobbyVisualizer(LobbyInterface lobby)
        {
            SetLobby(lobby);
        }

        public void SetLobby(LobbyInterface lobby)
        {
            DrawableLobby = lobby;
        }

        protected override void PostInitializing()
        {
            return;
        }

        protected override bool CanDraw()
        {
            return DrawableLobby != null;
        }

        protected override Color GetBackgroundColor
        {
            get { return DrawableLobby.GetBackgroundColor; }
        }

        protected override SpriteFont GetFont
        {
            get { return Content.Load<SpriteFont>("Lobby"); }
        }

        protected override void DrawVisualizer(GameTime gameTime)
        {
            DrawableString[] drawStrings = DrawableLobby.GetDrawableStrings;
            foreach (DrawableString drawString in drawStrings)
            {
                DrawString(drawString);
            }
        }

        protected void DrawString(DrawableString drawString)
        {
            SpriteBatchMain.DrawString(Font, drawString.Value, new Vector2(drawString.PosX, drawString.PosY), drawString.StringColor);
        }


    }

 
}
