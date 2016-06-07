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
            BallPosition = new Vector2(GameInitializers.BALL_POSX, GameInitializers.BALL_POSY);
            BallRadius = GameInitializers.BALL_RADIUS;

            PlayerData = new PlayerDrawingData[PLAYER_COUNT];

            PlayerData[0] = new PlayerDrawingData();
            PlayerData[0].Position = new Vector2(GameInitializers.PLAYER_1_X, GameInitializers.PLAYER_Y);
            PlayerData[0].BorderSize = new Vector2(GameInitializers.PLAYER_WIDTH, GameInitializers.PLAYER_HEIGHT);

            PlayerData[1] = new PlayerDrawingData();
            PlayerData[1].Position = new Vector2(GameInitializers.PLAYER_2_X, GameInitializers.PLAYER_Y);
            PlayerData[1].BorderSize = new Vector2(GameInitializers.PLAYER_WIDTH, GameInitializers.PLAYER_HEIGHT);

            BorderSize = new Vector2(GameInitializers.BORDER_WIDTH, GameInitializers.BORDER_HEIGHT);
        }

        public void DrawGame()
        {
            DrawBegin();
            DrawBorders();
            DrawBall();
            
            for (int ID = 0; ID < PLAYER_COUNT; ID++)
            {
                DrawPlayer(ID);
            }

            DrawEnd();
        }

        public abstract void Initialize(Game game);

        public abstract void LoadContent();

        public abstract void DrawBegin();

        public abstract void DrawBorders();

        public abstract void DrawBall();

        public abstract void DrawPlayer(int playerID);

        public abstract void DrawEnd();
    }

    class PlayerDrawingData
    {
        public Vector2 Position { get; set; }
        public Vector2 BorderSize { get; set; }
    }
}
