namespace GameLogicLibrary.GameObjects
{
    public class PlayBall
    {
        public float PosX { get; set; }
        public float PosY { get; set; }

        public int Radius { get; set; }

        public PlayBall()
        {
            PosX = GameInitializers.BALL_POSX;
            PosY = GameInitializers.BALL_POSY;

            Radius = GameInitializers.BALL_RADIUS;
        }
    }
}
