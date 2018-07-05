using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

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

            foreach (SelectionListInterface list in DrawableLobby.GetSelectionLists)
            {
                DrawSelectionList(list);
            }

            foreach (Animation animation in DrawableLobby.GetAnimationList)
            {
                animation.Update();
                animation.Draw(this);
            }
        }
    }
}
