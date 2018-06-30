using System;

namespace GameLogicLibrary
{
    public class GameInitializers
    {
        public const int BORDER_HEIGHT = 200;
        public const int BORDER_WIDTH = 500;

        public const int SCORE_NEEDED_FOR_VICTORY = 20;

        public const int BALL_RADIUS = 5;
        public const int BALL_POSX = BORDER_WIDTH / 2;
        public const int BALL_POSY = BORDER_HEIGHT / 2;
        public const float BALL_SPEED = 0.12F;
        public const float BALL_DIRX = 0.1F;
        public const float BALL_DIRY = 0.1F;

        public const int PLAYER_HEIGHT = 50;
        public const int PLAYER_WIDTH = 5;
        public const int PLAYER_SPACING = 30;
        public const int PLAYER_XOFFSET = 30;
        public const float PLAYER_SPEED = 1.5F;
        
        public const float MAXIMUM_ANGLE_DEGREE = 70;
        public static readonly float MAXIMUM_ANGLE_RAD = GameMath.DegreeToRadian(MAXIMUM_ANGLE_DEGREE);

        public static int GetPlayerX(int team, int playerIndex)
        {
            if (team == 0)
                return PLAYER_XOFFSET + (playerIndex * PLAYER_SPACING);
            else
                return BORDER_WIDTH - PLAYER_XOFFSET - PLAYER_WIDTH - (playerIndex * PLAYER_SPACING);
        }

        public static int GetPlayerY(int teamSize)
        {
            return BORDER_HEIGHT / 2 - GetPlayerHeight(teamSize) / 2;
        }

        public static int GetPlayerHeight(int teamSize)
        {
            return (int)(PLAYER_HEIGHT * Math.Pow(0.8, (teamSize > 0 ? teamSize - 1 : 0)));
        }
    }
}
