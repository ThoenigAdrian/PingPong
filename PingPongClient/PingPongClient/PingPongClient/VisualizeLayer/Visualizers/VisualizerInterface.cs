using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer.Visualizers
{
    public abstract class VisualizerInterface
    {
        protected GraphicsDeviceManager GraphicManager { get; private set; }
        protected ContentManager Content { get; private set; }
        protected GraphicsDevice Graphics { get { return GraphicManager.GraphicsDevice; } }
        protected SpriteBatch SpriteBatchMain { get; private set; }
        protected SpriteFont Font { get; set; }

        bool m_initialized = false;

        public void Initialize(XNAInitializationData initData)
        {
            GraphicManager = initData.GraphicManager;
            Content = initData.Content;
            SpriteBatchMain = initData.SpriteBatch;

            Font = GetFont;

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
    }
}
