using GameLogicLibrary;
using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    class FinishAnimation : Animation
    {
        const int PLAYER_SPACING = 120;

        Rectangle PlayerLeft;
        Rectangle PlayerRight;
        Vector2 BallPosition;
        int BallRadius = 6;
        int Direction = -1;
        bool PlayersCentered;


        public FinishAnimation(Vector2 center)
        {
            Center = center;
            Reset();
        }

        public override void Reset()
        {
            PlayersCentered = false;
            Initialize();
        }

        private void Initialize()
        {
            BallPosition = Center;

            PlayerLeft = new Rectangle(
                (int)Center.X - (PLAYER_SPACING / 2) - GameInitializers.PLAYER_WIDTH,
                (int)Center.Y - (GameInitializers.PLAYER_HEIGHT / 2),
                GameInitializers.PLAYER_WIDTH,
                GameInitializers.PLAYER_HEIGHT);

            PlayerRight = new Rectangle(
                (int)Center.X + (PLAYER_SPACING / 2),
                (int)Center.Y - (GameInitializers.PLAYER_HEIGHT / 2),
                GameInitializers.PLAYER_WIDTH,
                GameInitializers.PLAYER_HEIGHT);
        }

        public override void Update()
        {
            BallPosition.X += (1.2F * Direction);

            if (BallPosition.X - BallRadius < PlayerLeft.Right)
                Direction = 1;
            else if (BallPosition.X + BallRadius > PlayerRight.Left)
                Direction = -1;
        }

        public override void Draw(VisualizerInterface visualizer)
        {
            if (!PlayersCentered)
            {
                Center.X = visualizer.GetCenter().X;
                Initialize();
                PlayersCentered = true;
            }

            //int ballRadius = 6;
            //float BallPosX = BallPosition.X - ballRadius;
            //float BallPosY = BallPosition.Y - ballRadius;

            //SpriteBatchMain.Draw(BallTexture, new Rectangle(BallPosX, BallPosY, BallRadius * 2, BallRadius * 2), Color.Black);

            visualizer.DrawCircle(BallRadius, BallPosition, Color.Black);
            visualizer.DrawRectangle(PlayerLeft, Color.Black);
            visualizer.DrawRectangle(PlayerRight, Color.Black);
        }
    }
}
