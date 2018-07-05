using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Visualizers
{
    public abstract class VisualizerInterface
    {
        protected GraphicsDeviceManager GraphicManager { get; private set; }
        protected ContentManager Content { get; private set; }
        protected GraphicsDevice Graphics { get { return GraphicManager.GraphicsDevice; } }
        protected SpriteBatch SpriteBatchMain { get; private set; }
        protected SpriteFont Font { get; set; }

        static Texture2D CircleSelector;
        static Texture2D TextureRectangle;

        bool m_initialized = false;

        public void Initialize(XNAInitializationData initData)
        {
            GraphicManager = initData.GraphicManager;
            Content = initData.Content;
            SpriteBatchMain = initData.SpriteBatch;

            Font = GetFont;

            CircleSelector = TextureFactory.CreateCircleTexture(100, Graphics);
            TextureRectangle = TextureFactory.CreateRectangeTexture(Graphics);

            PostInitializing();

            m_initialized = true;
        }

        public void Draw(GameTime gameTime)
        {
            if (m_initialized && CanDraw())
            {
                DrawBegin();
                DrawVisualizer(gameTime);
                DrawEnd();
            }
        }

        protected abstract void PostInitializing();
        protected abstract bool CanDraw();
        protected abstract Color GetBackgroundColor { get; }
        protected abstract SpriteFont GetFont { get; }
        protected abstract void DrawVisualizer(GameTime gameTime);

        void DrawBegin()
        {
            DrawBackground();
            SpriteBatchMain.Begin();
        }

        void DrawEnd()
        {
            SpriteBatchMain.End();
        }

        void DrawBackground()
        {
            Graphics.Clear(GetBackgroundColor);
        }

        protected void DrawString(DrawableString drawString)
        {
            if (drawString.Visible)
            {
                float x;
                if (drawString.Options.DrawCentered)
                    x = GetCenteredXValue(drawString);
                else
                    x = drawString.Options.Position.X;

                SpriteBatchMain.DrawString(
                    Font,
                    drawString.Value,
                    new Vector2(x, drawString.Options.Position.Y),
                    drawString.Options.StringColor);
            }
        }

        private float GetCenteredXValue(DrawableString drawString)
        {
            return GetCenter().X - (drawString.GetMeasurements(Font).X / 2);
        }

        protected void DrawSelectionList(SelectionListInterface selectionList)
        {
            if (!selectionList.Visible)
                return;

            DrawRectangle(selectionList.TopLeft, selectionList.GetMeasurements(Font), selectionList.Background);

            int selection = 0;
            foreach (SelectionEntry entry in selectionList.ListEntries)
            {
                if (selection == selectionList.Selection)
                    DrawSelector(entry.Selector, selectionList.TopLeft + entry.Position + entry.SelectorPosition(Font));

                entry.DrawString.Options.Position = selectionList.TopLeft + entry.Position + entry.StringPosition(Font);
                DrawString(entry.DrawString);
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

        public void DrawCircle(int radius, Vector2 position, Color color)
        {
            SpriteBatchMain.Draw(CircleSelector,
                new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    2 * radius,
                    2 * radius),
                color);
        }

        public Vector2 GetCenter()
        {
            return new Vector2(Graphics.Viewport.Width / 2, Graphics.Viewport.Height / 2);
        }
    }
}
