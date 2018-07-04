using Microsoft.Xna.Framework;
using GameLogicLibrary.GameObjects;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Visualizers
{
    class GameStructureVisualizer : VisualizerInterface
    {
        BasicStructure Structure { get; set; }
        List<Player> Players { get { return Structure.Players; } }
        Ball Ball { get { return Structure.Ball; } }

        Texture2D FieldTexture;
        Texture2D BallTexture;
        Texture2D PlayerTexture;

        public Vector2 FieldSize { get { return new Vector2(Structure.Field.Width, Structure.Field.Height); } }


        DrawingOffsetTranslation DrawingTranslation;


        public GameStructureVisualizer()
        {
            DrawingTranslation = new DrawingOffsetTranslation();
        }

        public void SetGameStructure(BasicStructure structure)
        {
            Structure = structure;

            ApplyResize();
        }

        protected override Color GetBackgroundColor { get { return Color.Black; } }

        protected override SpriteFont GetFont { get { return Content.Load<SpriteFont>("Game"); } }

        protected override void PostInitializing()
        {
            CreateObjectTextures();
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
            DrawScore();
            DrawBorders();
            DrawBall();
            
            foreach(Player player in Players)
            {
                DrawPlayer(player);
            }
        }

        protected override bool CanDraw()
        {
            return Structure != null;
        }

        protected void DrawScore()
        {
            DrawableString scoreString = new DrawableString("Team 1   " + Structure._score.Score_Team1 + "-" + Structure._score.Score_Team2 + "   Team 2",
                new Vector2(0, 10),
                Color.White);

            Vector2 measurements = scoreString.GetMeasurements(Font);
            float x = DrawingTranslation.GetAbsoluteX(FieldSize.X / 2);
            x -= measurements.X / 2;
            scoreString.Postion.X = x;

            DrawString(scoreString);
        }

        protected void DrawBall()
        {
            int BallPosX = (int)DrawingTranslation.GetAbsoluteX(Ball.PositionX - Ball.Radius);
            int BallPosY = (int)DrawingTranslation.GetAbsoluteY(Ball.PositionY - Ball.Radius);
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

        protected void DrawPlayer(Player player)
        {
            int playerPosX = (int)DrawingTranslation.GetAbsoluteX(player.PositionX);
            int playerPosY = (int)DrawingTranslation.GetAbsoluteY(player.PositionY);
            int playerWidth = (int)DrawingTranslation.GetAbsoluteSize(player.Width);
            int playerHeight = (int)DrawingTranslation.GetAbsoluteSize(player.Height);


            SpriteBatchMain.Draw(PlayerTexture, new Rectangle(playerPosX, playerPosY, playerWidth, playerHeight), Color.Black);
        }
    }
}
