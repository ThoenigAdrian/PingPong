using System;

namespace GameLogicLibrary.GameObjects
{
    public class RawBall
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float DirectionX { get; set; }
        public float DirectionY { get; set; }
    }

    public class Ball : RawBall
    {
        public int Radius { get; set; }
        public int LastTouchedTeam;
        private float internal_speed;

        public Ball()
        {
            PositionX = GameInitializers.BALL_POSX;
            PositionY = GameInitializers.BALL_POSY;
            DirectionX = GameInitializers.BALL_DIRX;
            DirectionY = GameInitializers.BALL_DIRY;
            Speed = 1F;
            LastTouchedTeam = -1; // = No Team        + refactor Teams stuff e.g Team Blue , Team Red or something different but it doesn't make sense to have a enum like this (Team1,Team2,Team3)
            Radius = GameInitializers.BALL_RADIUS;
        }

        public float Speed
        {
            get
            {
                return (float) Math.Sqrt(DirectionX * DirectionX + DirectionY * DirectionY);
            }
            set
            {
                internal_speed = value;
                DirectionX = DirectionX * internal_speed;
                DirectionY = DirectionY * internal_speed;
                
            }
        }
    }
}
