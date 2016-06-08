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

        Texture2D FieldTexture;
        Texture2D BallTexture;
        Texture2D PlayerTexture;

        DrawingOffsetTranslation DrawingTranslation;

        public XNAGameVisualizer(GameStructure structure) : base(structure)
        {
            DrawingTranslation = new DrawingOffsetTranslation();
        }

        public override void Initialize(Game game)
        {
            GraphicManager = new GraphicsDeviceManager(game);
            Content = new ContentManager(game.Services);
            Content.RootDirectory = "Content";
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

            DrawingTranslation.Scaling = 1.3F;

            int screenHeight = GraphicManager.PreferredBackBufferHeight;
            int screenWidth = GraphicManager.PreferredBackBufferWidth;
            DrawingTranslation.DrawingOffset = new Vector2(
                screenWidth / 2 - DrawingTranslation.GetAbsoluteSize(FieldSize.X / 2), 
                screenHeight / 2 - DrawingTranslation.GetAbsoluteSize(FieldSize.Y / 2));
        }

        protected override void DrawBegin()
        {
            Graphics.Clear(Color.Black);
            SpriteBatchMain.Begin();
        }

        protected override void DrawBall()
        {
            int BallPosX = (int)DrawingTranslation.GetAbsoluteX(Ball.PosX - Ball.Radius);
            int BallPosY = (int)DrawingTranslation.GetAbsoluteY(Ball.PosY - Ball.Radius);
            int BallRadius = (int)DrawingTranslation.GetAbsoluteSize(Ball.Radius);

            SpriteBatchMain.Draw(BallTexture, new Rectangle(BallPosX, BallPosY, BallRadius * 2, BallRadius * 2), Color.Black);
        }

        protected override void DrawBorders()
        {
            SpriteBatchMain.Draw(
                FieldTexture, 
                new Rectangle(
                    (int)DrawingTranslation.GetAbsoluteX(0), 
                    (int)DrawingTranslation.GetAbsoluteY(0), 
                    (int)DrawingTranslation.GetAbsoluteSize(FieldSize.X), 
                    (int)DrawingTranslation.GetAbsoluteSize(FieldSize.Y)), 
                Color.White);
        }

        protected override void DrawPlayer(PlayerBar player)
        {
            int playerPosX = (int)DrawingTranslation.GetAbsoluteX(player.PosX);
            int playerPosY = (int)DrawingTranslation.GetAbsoluteY(player.PosY);
            int playerWidth = (int)DrawingTranslation.GetAbsoluteSize(player.Width);
            int playerHeight = (int)DrawingTranslation.GetAbsoluteSize(player.Height);


            SpriteBatchMain.Draw(PlayerTexture, new Rectangle(playerPosX, playerPosY, playerWidth, playerHeight), Color.Black);
        }

        protected override void DrawEnd()
        {
            SpriteBatchMain.End();
        }
    }
}
