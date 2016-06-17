namespace GameLogicLibrary.GameObjects
{
    public class PlayBall
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float DirectionX { get; set; }
        public float DirectionY { get; set; }

        public int Radius { get; set; }

        public PlayBall()
        {
            PositionX = GameInitializers.BALL_POSX;
            PositionY = GameInitializers.BALL_POSY;
            DirectionX = GameInitializers.BALL_DIRX;
            DirectionY = GameInitializers.BALL_DIRY;
            Radius = GameInitializers.BALL_RADIUS;
        }
    }
}
