using Microsoft.Xna.Framework;
using GameLogicLibrary;
using System.Collections.Generic;

namespace PingPongClient.VisualizeLayer
{
    abstract class GameVisualizerInterface
    {
        const int PLAYER_COUNT = 2;

        public Vector2 BallPosition { get; set; }
        public int BallRadius { get; set; }

        protected PlayerDrawingData[] PlayerData { get; private set; }
        
        public Vector2 Player1Position
        {
            get { return PlayerData[0].Position; }
            set { PlayerData[0].Position = value; }
        }

        public Vector2 Player1BorderSize
        {
            get { return PlayerData[0].BorderSize; }
            set { PlayerData[0].Position = value; }
        }

        public Vector2 Player2Position
        {
            get { return PlayerData[1].Position; }
            set { PlayerData[1].Position = value; }
        }

        public Vector2 Player2BorderSize
        {
            get { return PlayerData[1].BorderSize; }
            set { PlayerData[1].Position = value; }
        }

        public Vector2 BorderSize { get; set; }

        protected GameVisualizerInterface()
        {
            BallPosition = new Vector2();
            BallRadius = GameInitializers.BALL_RADIUS;

            PlayerData = new PlayerDrawingData[PLAYER_COUNT];

            for (int i = 0; i < PLAYER_COUNT; i++)
            {
                PlayerData[i] = new PlayerDrawingData();
                PlayerData[i].Position = new Vector2();
                PlayerData[i].BorderSize = new Vector2(GameInitializers.PLAYER_WIDTH, GameInitializers.PLAYER_HEIGHT);
            }

            BorderSize = new Vector2(GameInitializers.BORDER_WIDTH, GameInitializers.BORDER_HEIGHT);
        }

        public void DrawGame()
        {
            DrawBorders();
            DrawBall();
            
            for (int ID = 0; ID < PLAYER_COUNT; ID++)
            {
                DrawPlayer(ID);
            }
        }

        public abstract void DrawBorders();

        public abstract void DrawBall();

        public abstract void DrawPlayer(int playerID);
    }

    class PlayerDrawingData
    {
        public Vector2 Position { get; set; }
        public Vector2 BorderSize { get; set; }
    }
}
