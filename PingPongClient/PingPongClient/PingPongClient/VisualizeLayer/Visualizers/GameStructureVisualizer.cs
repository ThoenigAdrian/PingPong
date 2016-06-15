using Microsoft.Xna.Framework;
using GameLogicLibrary.GameObjects;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer.Visualizers
{
    class GameStructureVisualizer : VisualizerInterface
    {
        GameStructure Structure { get; set; }
        Player[] Players { get { return Structure.GetAllPlayers(); } }
        PlayBall Ball { get { return Structure.Ball; } }

        Texture2D FieldTexture;
        Texture2D BallTexture;
        Texture2D PlayerTexture;

        public Vector2 FieldSize { get { return new Vector2(Structure.GameField.Width, Structure.GameField.Height); } }


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
            
            foreach(Player player in Players)
            {
                DrawPlayer(player.PlayerBar);
            }
        }

        protected override bool CanDraw()
        {
            return Structure != null;
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

        protected void DrawPlayer(PlayerBar player)
        {
            int playerPosX = (int)DrawingTranslation.GetAbsoluteX(player.PositionX);
            int playerPosY = (int)DrawingTranslation.GetAbsoluteY(player.PositionY);
            int playerWidth = (int)DrawingTranslation.GetAbsoluteSize(player.Width);
            int playerHeight = (int)DrawingTranslation.GetAbsoluteSize(player.Height);


            SpriteBatchMain.Draw(PlayerTexture, new Rectangle(playerPosX, playerPosY, playerWidth, playerHeight), Color.Black);
        }
    }
}
