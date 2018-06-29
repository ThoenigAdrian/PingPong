using System;

namespace GameLogicLibrary.GameObjects
{
    public class RawBall
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float DirectionX { get; protected set; }
        public float DirectionY { get; protected set; }
    }

    public class Ball : RawBall
    {
        public int Radius { get; set; }
        public int LastTouchedTeam;
        private float internal_speed = GameInitializers.BALL_SPEED;

        public Ball()
        {
            PositionX = GameInitializers.BALL_POSX;
            PositionY = GameInitializers.BALL_POSY;
            DirectionX = GameInitializers.BALL_DIRX;
            DirectionY = GameInitializers.BALL_DIRY;
            Speed = GameInitializers.BALL_SPEED;
            LastTouchedTeam = -1; // = No Team        + refactor Teams stuff e.g Team Blue , Team Red or something different but it doesn't make sense to have a enum like this (Team1,Team2,Team3)
            Radius = GameInitializers.BALL_RADIUS;
        }

        public void ReverseDirectionX()
        {
            DirectionX *= -1;
        }

        public void ReverseDirectionY()
        {
            DirectionY *= -1;
        }

        public void resetToInitialSpeed()
        {
            Speed = GameInitializers.BALL_SPEED;
        }

        public void increaseSpeed(TimeSpan timePassed)
        {
            Speed = GameInitializers.BALL_SPEED + ((float)timePassed.TotalSeconds / 180);
        }

        public void ChangeAngleOfBall(float Angle)
        {
            DirectionX = (float)Math.Cos(Angle) * internal_speed;
            DirectionY = (float)Math.Sin(Angle) * internal_speed;
        }

        private void Normalize()
        {
            float distance = (float)Math.Sqrt(DirectionX * DirectionX + DirectionY * DirectionY);
            DirectionX = DirectionX / distance;
            DirectionY = DirectionY / distance;
        }


        public float Speed
        {
            get { return internal_speed; }
            set
            {
                internal_speed = value;
                Normalize();
                DirectionX *= internal_speed;
                DirectionY *= internal_speed;
            }
        }
    }
}
