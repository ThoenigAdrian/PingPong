namespace GameLogicLibrary.GameObjects
{
    public class PlayBall
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }

        public int Radius { get; set; }

        public PlayBall()
        {
            PositionX = GameInitializers.BALL_POSX;
            PositionY = GameInitializers.BALL_POSY;

            Radius = GameInitializers.BALL_RADIUS;
        }
    }
}
