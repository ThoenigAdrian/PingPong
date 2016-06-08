using GameLogicLibrary.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PingPongClient.VisualizeLayer
{
    class XNAGameVisualizer : GameVisualizerInterface
    {
        GraphicsDeviceManager GraphicManager { get; set; }
        SpriteBatch SpriteBatchMain { get; set; }
        ContentManager Content { get; set; }
        GraphicsDevice Graphics { get { return GraphicManager.GraphicsDevice; } }

        Vector2 DrawingOffset;

        Texture2D FieldTexture;
        Texture2D BallTexture;
        Texture2D PlayerTexture;

        public XNAGameVisualizer(GameStructure structure) : base(structure)
        {

        }

        public override void Initialize(Game game)
        {
            GraphicManager = new GraphicsDeviceManager(game);
            Content = new ContentManager(game.Services);
            Content.RootDirectory = "Content";

            DrawingOffset = new Vector2(0, 0);
        }

        public override void LoadContent()
        {
            SpriteBatchMain = new SpriteBatch(Graphics);
            CreateObjectTextures();

            ApplyResize();
        }

        private void CreateObjectTextures()
        {
            FieldTexture = TextureFactory.CreateRectangeTexture(Graphics);
            BallTexture = TextureFactory.CreateCircleTexture(100, Graphics);
            PlayerTexture = TextureFactory.CreateRectangeTexture(Graphics);
        }

        public override void ApplyResize()
        {
            if (GraphicManager == null)
                return;

            int screenHeight = GraphicManager.PreferredBackBufferHeight;
            int screenWidth = GraphicManager.PreferredBackBufferWidth;
            DrawingOffset.X = screenWidth / 2 - FieldSize.X / 2;
            DrawingOffset.Y = screenHeight / 2 - FieldSize.Y / 2;
        }

        protected override void DrawBegin()
        {
            Graphics.Clear(Color.Black);
            SpriteBatchMain.Begin();
        }

        protected override void DrawBall()
        {
            int BallPosX = (int)GetAbsoluteX(Ball.PosX - Ball.Radius);
            int BallPosY = (int)GetAbsoluteY(Ball.PosY - Ball.Radius);

            SpriteBatchMain.Draw(BallTexture, new Rectangle(BallPosX, BallPosY, Ball.Radius * 2, Ball.Radius * 2), Color.Black);
        }

        protected override void DrawBorders()
        {
            SpriteBatchMain.Draw(FieldTexture, new Rectangle((int)GetAbsoluteX(0), (int)GetAbsoluteY(0), (int)FieldSize.X, (int)FieldSize.Y), Color.White);
        }

        protected override void DrawPlayer(PlayerBar player)
        {
            int playerPosX = (int)GetAbsoluteX(player.PosX);
            int playerPosY = (int)GetAbsoluteY(player.PosY);

            SpriteBatchMain.Draw(PlayerTexture, new Rectangle(playerPosX, playerPosY, (int)player.Width, (int)player.Height), Color.Black);
        }

        protected override void DrawEnd()
        {
            SpriteBatchMain.End();
        }

        Vector2 GetAbsolutePoint(Vector2 relativePoint)
        {
            return new Vector2(DrawingOffset.X + relativePoint.X, DrawingOffset.Y + relativePoint.Y);
        }

        float GetAbsoluteX(float relativeX)
        {
            return DrawingOffset.X + relativeX;
        }

        float GetAbsoluteY(float relativeY)
        {
            return DrawingOffset.Y + relativeY;
        }
    }
}
