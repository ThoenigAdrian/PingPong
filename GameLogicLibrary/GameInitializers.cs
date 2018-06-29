using System;

namespace GameLogicLibrary
{
    public class GameInitializers
    {
        public const int BORDER_HEIGHT = 200;
        public const int BORDER_WIDTH = 500;

        public const int BALL_RADIUS = 5;
        public const int BALL_POSX = 250;
        public const int BALL_POSY = 100;
        public const float BALL_DIRX = 0.1F;
        public const float BALL_DIRY = 0.1F;

        public const int PLAYER_HEIGHT = 50;
        public const int PLAYER_WIDTH = 5;
        public const int PLAYER_SPACING = 20;
        public const int PLAYER_Y = BORDER_HEIGHT / 2 - PLAYER_HEIGHT / 2;
        public const int PLAYER_XOFFSET = 30;
        public const float PLAYER_SPEED = 1.5F;
        public const float MAXIMUM_ANGLE_DEGREE = 70;
        public const float MAXIMUM_ANGLE_RAD = (float)(Math.PI * MAXIMUM_ANGLE_DEGREE / 180);

        public static int GetPlayerX(int team, int playerIndex)
        {
            if (team == 0)
                return PLAYER_XOFFSET + (playerIndex * PLAYER_SPACING);
            else
                return (BORDER_WIDTH - PLAYER_XOFFSET - PLAYER_WIDTH) - (playerIndex * PLAYER_SPACING);
        }
    }
}
