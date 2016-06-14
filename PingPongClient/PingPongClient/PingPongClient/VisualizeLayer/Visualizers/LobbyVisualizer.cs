using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Lobbies.SelectionLists;

namespace PingPongClient.VisualizeLayer.Visualizers
{
    class LobbyVisualizer : VisualizerInterface
    {
        LobbyInterface DrawableLobby;
        Texture2D CircleSelector;
        Texture2D TextureRectangle;

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
            CircleSelector = TextureFactory.CreateCircleTexture(100, Graphics);
            TextureRectangle = TextureFactory.CreateRectangeTexture(Graphics);
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
        }

        protected void DrawString(DrawableString drawString)
        {
            DrawString(drawString, drawString.Postion);
        }

        protected void DrawString(DrawableString drawString, Vector2 position)
        {
            SpriteBatchMain.DrawString(Font, drawString.Value, position, drawString.StringColor);
        }

        protected void DrawSelectionList(SelectionListInterface selectionList)
        {
            DrawRectangle(selectionList.TopLeft, selectionList.GetMeasurements(Font), selectionList.Background);

            int selection = 0;
            foreach (SelectionEntry entry in selectionList.ListEntries)
            {
                if (selection == selectionList.Selection)
                    DrawSelector(entry.Selector, selectionList.TopLeft + entry.Position + entry.SelectorPosition(Font));

                DrawString(entry.DrawString, selectionList.TopLeft + entry.Position + entry.StringPosition(Font));
                selection++;
            }
        }

        protected void DrawRectangle(Vector2 position, Vector2 size, Color color)
        {
            SpriteBatchMain.Draw(TextureRectangle, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), color);
        }

        protected void DrawSelector(Selector selector, Vector2 position)
        {
            SpriteBatchMain.Draw(CircleSelector,
                new Rectangle(
                    (int)position.X,
                    (int)position.Y, 
                    (int)selector.Size.X, 
                    (int)selector.Size.Y),
                selector.SelectorColor);
        }
    }
}
