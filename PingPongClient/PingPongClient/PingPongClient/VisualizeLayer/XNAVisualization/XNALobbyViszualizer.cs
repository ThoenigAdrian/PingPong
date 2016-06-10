using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PingPongClient.VisualizeLayer.XNAVisualization
{
    class XNALobbyVisualizer : LobbyVisualizer, XNAVisualizer
    {
        GraphicsDeviceManager GraphicManager { get; set; }
        SpriteBatch SpriteBatchMain { get; set; }
        ContentManager Content { get; set; }
        GraphicsDevice Graphics { get { return GraphicManager.GraphicsDevice; } }

        SpriteFont Font { get; set; }

        bool m_initialized = false;

        void XNAVisualizer.Initialize(XNAInitializationData initData)
        {
            GraphicManager = initData.GraphicManager;
            Content = initData.Content;
            SpriteBatchMain = initData.SpriteBatch;

            Font = Content.Load<SpriteFont>("Lobby");

            m_initialized = true;
        }

        protected override void DrawBegin()
        {
            Graphics.Clear(Color.Black);
            SpriteBatchMain.Begin();
        }

        protected override void DrawString(DrawableString drawString)
        {
            SpriteBatchMain.DrawString(Font, drawString.Value, new Vector2(drawString.PosX, drawString.PosY), Color.White);
        }

        protected override void DrawEnd()
        {
            SpriteBatchMain.End();
        }

        protected override bool CanDraw()
        {
            return m_initialized;
        }
    }
}
