using Microsoft.Xna.Framework;
using System.Collections.Generic;
using GameLogicLibrary.GameObjects;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer.Visualizers
{
    class GameStructureVisualizer : VisualizerInterface
    {
        GameStructure Structure { get; set; }
        List<PlayerBar> Players { get { return Structure.m_players; } }
        PlayBall Ball { get { return Structure.m_ball; } }

        Texture2D FieldTexture;
        Texture2D BallTexture;
        Texture2D PlayerTexture;

        public Vector2 FieldSize { get { return new Vector2(Structure.m_field.Width, Structure.m_field.Height); } }


        DrawingOffsetTranslation DrawingTranslation;


        public GameStructureVisualizer()
        {
            DrawingTranslation = new DrawingOffsetTranslation();
        }

        public void SetGameStructure(GameStructure structure)
        {
            Structure = structure;

            ApplyResize();
        }

        protected override Color GetBackgroundColor { get { return Color.Black; } }

        protected override SpriteFont GetFont { get { return null; } }

        protected override void PostInitializing()
        {
            CreateObjectTextures();

            ApplyResize();
        }

        public void ApplyResize()
        {
            if (GraphicManager == null)
                return;

            DrawingTranslation.Scaling = 1.5F;

            int screenHeight = GraphicManager.PreferredBackBufferHeight;
            int screenWidth = GraphicManager.PreferredBackBufferWidth;
            DrawingTranslation.DrawingOffset = new Vector2(
                screenWidth / 2 - DrawingTranslation.GetAbsoluteSize(FieldSize.X / 2),
                screenHeight / 2 - DrawingTranslation.GetAbsoluteSize(FieldSize.Y / 2));
        }

        private void CreateObjectTextures()
        {
            FieldTexture = TextureFactory.CreateRectangeTexture(Graphics);
            BallTexture = TextureFactory.CreateCircleTexture(100, Graphics);
            PlayerTexture = TextureFactory.CreateRectangeTexture(Graphics);
        }

        protected override void DrawVisualizer(GameTime gameTime)
        {
            DrawBorders();
            DrawBall();
            
            foreach(PlayerBar player in Players)
            {
                DrawPlayer(player);
            }
        }

        protected override bool CanDraw()
        {
            return Structure != null;
        }

        protected void DrawBall()
        {
            int BallPosX = (int)DrawingTranslation.GetAbsoluteX(Ball.PosX - Ball.Radius);
            int BallPosY = (int)DrawingTranslation.GetAbsoluteY(Ball.PosY - Ball.Radius);
            int BallRadius = (int)DrawingTranslation.GetAbsoluteSize(Ball.Radius);

            SpriteBatchMain.Draw(BallTexture, new Rectangle(BallPosX, BallPosY, BallRadius * 2, BallRadius * 2), Color.Black);
        }

        protected void DrawBorders()
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

        protected void DrawPlayer(PlayerBar player)
        {
            int playerPosX = (int)DrawingTranslation.GetAbsoluteX(player.PosX);
            int playerPosY = (int)DrawingTranslation.GetAbsoluteY(player.PosY);
            int playerWidth = (int)DrawingTranslation.GetAbsoluteSize(player.Width);
            int playerHeight = (int)DrawingTranslation.GetAbsoluteSize(player.Height);


            SpriteBatchMain.Draw(PlayerTexture, new Rectangle(playerPosX, playerPosY, playerWidth, playerHeight), Color.Black);
        }
    }
}
