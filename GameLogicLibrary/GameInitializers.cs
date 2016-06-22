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
        public const int PLAYER_Y = BORDER_HEIGHT / 2 - PLAYER_HEIGHT / 2;
        public const int PLAYER_1_X = 30;
        public const int PLAYER_2_X = BORDER_WIDTH - PLAYER_1_X - PLAYER_WIDTH;
        public const float PLAYER_SPEED = 2;
    }
}
