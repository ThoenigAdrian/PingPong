using System;
using System.Threading;
using System.Collections.Generic;
using GameLogicLibrary.GameObjects;
using XSLibrary.Utility;

namespace GameLogicLibrary
{
    public class GameEngine
    {
        private GameStructure GameStructure;

        public delegate void TeamScoredEventHandler(object sender, EventArgs e);
        public event TeamScoredEventHandler TeamScored;

        OneShotTimer GameOngoingTimer;

        Random random;

        public GameEngine(GameStructure GameStructure)
        {
            this.GameStructure = GameStructure;
            random = new Random();
            ResetBall();
            RandomizeBallDirection();
        }

        private void ResetBall()
        {
            GameStructure.Ball.PositionX = GameInitializers.BALL_POSX;
            GameStructure.Ball.PositionY = GameInitializers.BALL_POSY;
            RandomizeBallDirection();
            GameStructure.Ball.LastTouchedTeam = -1;
            GameStructure.Ball.resetToInitialSpeed();
            GameOngoingTimer = new OneShotTimer(1000 * 1000, true);
        }

        private void RandomizeBallDirection()
        {
            int angle = random.Next(360);

            if (angle > 45 && angle < 90)
                angle -= 45;
            else if (angle > 90 && angle < 135)
                angle += 45;
            else if (angle > 225 && angle < 270)
                angle -= 45;
            else if (angle > 270 && angle < 315)
                angle += 45;

            float radiant = (float)angle / 180 * (float)Math.PI;
            GameStructure.Ball.ChangeAngleOfBall(angle);
        }

        private bool PointInRectangular(GameStructure.Point point, GameStructure.Rectangle rectangle)
        {
            bool betweenXLine = (rectangle.PositionX <= point.PositionX) && (point.PositionX <= rectangle.PositionX + rectangle.Width);
            bool betweenYLine = (rectangle.PositionY <= point.PositionY) && (point.PositionY <= rectangle.PositionY + rectangle.Height);
            return betweenXLine && betweenYLine;
        }

        private bool BallInPlayerBar(Player player)
        {
            GameStructure.Circle circle = new GameStructure.Circle(GameStructure.Ball.PositionX, GameStructure.Ball.PositionY, GameStructure.Ball.Radius);
            GameStructure.Rectangle rectangle = new GameStructure.Rectangle(player.PositionX, player.PositionY, player.Height, player.Width);
            return CircleInRect(circle, rectangle);
        }

        private bool CircleInRect(GameStructure.Circle circle, GameStructure.Rectangle rectangle)
        {
            GameStructure.Point lefmost = new GameStructure.Point(circle.PositionX - circle.Radius, circle.PositionY);
            GameStructure.Point rightmost = new GameStructure.Point(circle.PositionX + circle.Radius, circle.PositionY);
            GameStructure.Point topmost = new GameStructure.Point(circle.PositionX, circle.PositionY - circle.Radius);
            GameStructure.Point bottommost = new GameStructure.Point(circle.PositionX, circle.PositionY + circle.Radius);

            List<GameStructure.Point> cornerPoints = new List<GameStructure.Point>();
            cornerPoints.Add(lefmost);
            cornerPoints.Add(rightmost);
            cornerPoints.Add(topmost);
            cornerPoints.Add(bottommost);

            foreach (GameStructure.Point point in cornerPoints)
            {
                if (PointInRectangular(point, rectangle))
                    return true;
            }

            return false;
        }

        private float Max(Tuple<float, float> floatTuple)
        {
            return Math.Max(floatTuple.Item1, floatTuple.Item2);
        }

        private float Min(Tuple<float, float> floatTuple)
        {
            return Math.Min(floatTuple.Item1, floatTuple.Item2);
        }

        private void OnTeamScored()
        {
            ResetBall();

            TeamScored?.Invoke(this, EventArgs.Empty);
        }

        private void ChangeDirectionOfBall(float Angle)
        {
            if (GameStructure.Ball.DirectionX > 0)
            {
                GameStructure.Ball.ChangeAngleOfBall(Angle);
                GameStructure.Ball.ReverseDirectionX();
                
            }
            else
            {
                GameStructure.Ball.ChangeAngleOfBall(Angle);
            }

        }

        public void CalculateFrame(long timePassedInMilliseconds)
        {
            if (GameOngoingTimer == true)
            {
                GameStructure.Ball.PositionX += GameStructure.Ball.DirectionX * timePassedInMilliseconds;
                GameStructure.Ball.PositionY += GameStructure.Ball.DirectionY * timePassedInMilliseconds;
            }

            foreach (KeyValuePair<int, GameStructure.GameTeam> Team in GameStructure.GameTeams)
            {
                foreach (Player player in Team.Value.PlayerList)
                {
                    CalculatePlayerPosition(player);
                }
            }

            CalculateCollisions();
            IncreaseBallSpeed();
        }

        private void IncreaseBallSpeed()
        {
            GameStructure.Ball.increaseSpeed(GameOngoingTimer.TimePassed);
        }

        private void CalculateWallCollisions()
        {
            if (GameStructure.Ball.PositionX >= GameStructure.GameField.Width - GameStructure.Ball.Radius)
            {
                if (GameStructure.scoring)
                {
                    GameStructure.GameTeams[0].score++;
                    OnTeamScored();
                }

                else
                {
                   GameStructure.Ball.PositionX = (GameStructure.GameField.Width - GameStructure.Ball.Radius) - (GameStructure.Ball.PositionX - (GameStructure.GameField.Width - GameStructure.Ball.Radius));
                    GameStructure.Ball.ReverseDirectionX();
                }

            }

            if (GameStructure.Ball.PositionY >= GameStructure.GameField.Height - GameStructure.Ball.Radius)
            {
                GameStructure.Ball.PositionY = (GameStructure.GameField.Height - GameStructure.Ball.Radius) - (GameStructure.Ball.PositionY - (GameStructure.GameField.Height - GameStructure.Ball.Radius));
                GameStructure.Ball.ReverseDirectionY();
            }
            if (GameStructure.Ball.PositionX <= GameStructure.Ball.Radius)
            {
                if (GameStructure.scoring)
                {
                    GameStructure.GameTeams[1].score++;
                    OnTeamScored();
                }

                else
                {
                   GameStructure.Ball.PositionX =GameStructure.Ball.PositionX * -1;
                   GameStructure.Ball.ReverseDirectionX();
                }
            }

            if (GameStructure.Ball.PositionY <= GameStructure.Ball.Radius)
            {
                GameStructure.Ball.PositionY = GameStructure.Ball.Radius + (GameStructure.Ball.Radius - GameStructure.Ball.PositionY);
                GameStructure.Ball.ReverseDirectionY();
            }

        }

        private void CalculatePlayerPosition(Player player)
        {
            player.PositionY += player.DirectionY;

            if (player.PositionY >= GameInitializers.BORDER_HEIGHT - player.Height)
                player.PositionY = GameInitializers.BORDER_HEIGHT - player.Height;
            if (player.PositionY <= 0)
                player.PositionY = 0;
        }

        private void CalculateCollisions()
        {
            CalculatePlayerBallCollisions();
            CalculateWallCollisions();
        }

        private void CalculatePlayerBallCollisions()
        {
            foreach (KeyValuePair<int, GameStructure.GameTeam> Team in GameStructure.GameTeams)
            {
                foreach (Player player in Team.Value.PlayerList)
                {
                    if ((GameStructure.Ball.LastTouchedTeam == player.Team) && !GameStructure.friendlyFire)
                        continue;
                    if (BallInPlayerBar(player))
                    {
                        GameStructure.Ball.LastTouchedTeam = player.Team;
                        float Angle = GetNewAngleOfBall(player);
                        ChangeDirectionOfBall(Angle);
                        return; // ASSUMING ONLY ONE PLAYER CAN TOUCH THE BALL !
                    }

                }
            }
        }

        private float GetNewAngleOfBall(Player p)
        {
            return GameInitializers.MAXIMUM_ANGLE_RAD * GetRelativeDistanceFromMiddlePoint(p);
        }

        private float GetRelativeDistanceFromMiddlePoint(Player p)
        {
            float relativeDistance = (GameStructure.Ball.PositionY - (p.PositionY + p.Height / 2)) / (p.Height / 2);

            if (relativeDistance < -1)
                relativeDistance = -1;
            if (relativeDistance > 1)
                relativeDistance = 1;

            return relativeDistance;
        }
        

        /* THOUGHTS ABOUT A BETTER COLLISION DETECTION SYSTEM BUT MAYBE NOT NECESSARY
        private bool CircleInRectangle(Player p)
        {
            float minimumDistanceForCollision = (float)Math.Sqrt((Math.Pow(p.Height / 2, 2) + Math.Pow(p.Width / 2, 2))) + GameStructure.Ball.Radius;

            Tuple<float, float> PlayerBallYPositionTuple = Tuple.Create(GameStructure.Ball.PositionY, p.PositionY);
            float yDistance = Max(PlayerBallYPositionTuple) - Min(PlayerBallYPositionTuple);

            Tuple<float, float> PlayerBallXPositionTuple = Tuple.Create(GameStructure.Ball.PositionX, p.PositionX);
            float xDistance = Max(PlayerBallYPositionTuple) - Min(PlayerBallYPositionTuple);

            float actualDistance = (float)Math.Sqrt(Math.Pow(yDistance,2) + Math.Pow(xDistance,2 ));

            if (actualDistance > minimumDistanceForCollision)
                return false;                 
            
            if (actualDistance <= GameStructure.Ball.Radius)
                return true; // this is a special case the return true has to be substituted by a deeper looking algorithm which checks if those two are colliding
                             // XXX
                             //XX  XX
                             // XXX+ --+
                             //     |  |
                             //     |  |
                             //     |  |
                             //     |  |
                             //     |  |
                             //     |  |
                             //     |  |
                             //     |  |
                             //     |  |
                             //     |  |
                             //     +--+    //will be implemented later

            if (actualDistance < minimumDistanceForCollision)
            {
                if (p.PositionX >= GameInitializers.BORDER_WIDTH / 2)
                {
                    if (GameStructure.Ball.PositionX >= p.PositionX)
                    {
                        GameStructure.Ball.DirectionX = GameStructure.Ball.DirectionX * -1;
                        return true;
                    }
                        
                }
                if (p.PositionX <= GameInitializers.BORDER_WIDTH / 2)
                {
                    if (GameStructure.Ball.PositionX <= p.PositionX)
                    {
                        GameStructure.Ball.DirectionX = GameStructure.Ball.DirectionX * -1;
                        return true;
                    }

                }
            }
                
            
            return false;
        }
        */
    }
}
