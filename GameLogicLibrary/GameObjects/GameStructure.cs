using System.Collections.Generic;
using System;
using System.Threading;

namespace GameLogicLibrary.GameObjects
{
    public class GameStructure
    {
        BasicStructure Structure { get; set; }
        GameField GameField { get { return Structure.Field; } }
        public Ball Ball { get { return Structure.Ball; } }
        List<Player> Players { get { return Structure.Players; } }

        public Dictionary<int,GameTeam> GameTeams = new Dictionary<int, GameTeam>();
        public const int TEAM_COUNT = 2; // restrict to two teams for now
        public int maxPlayers;
        public bool friendlyFire = false;
        public bool scoring = true;

        public delegate void TeamScoredEventHandler(object sender, EventArgs e);
        public event TeamScoredEventHandler TeamScored;

        public int PlayersCount
        {
            get
            {
                int PlayersCount = 0;

                foreach(KeyValuePair<int, GameTeam> entry in GameTeams)
                    PlayersCount += entry.Value.PlayerList.Count;
                
                return PlayersCount;
            }
        }

        public int MissingPlayers
        {
            get
            {
               return maxPlayers - PlayersCount;
            }
        }

        public GameStructure(int maxPlayers)
        {
            Structure = new BasicStructure(new GameField(), new Ball());
            this.maxPlayers = maxPlayers;
            GameTeams.Add(0, new GameTeam());
            GameTeams.Add(1, new GameTeam());
        }

        public void CalculateFrame(long timePassedInMilliseconds)
        {

            Ball.PositionX += Ball.DirectionX * timePassedInMilliseconds;
            Ball.PositionY += Ball.DirectionY * timePassedInMilliseconds;

            foreach (KeyValuePair<int, GameTeam> Team in GameTeams)
            {
                foreach (Player player in Team.Value.PlayerList)
                {
                    CalculatePlayerPosition(player);
                }
            }          

            CalculateCollisions();
        }

        private void CalculateWallCollisions()
        {
            if (Ball.PositionX >= GameField.Width - Ball.Radius)
            {
                if (scoring)
                {
                    GameTeams[0].score++;
                    OnTeamScored();
                }

                else
                {
                    Ball.PositionX = (GameField.Width - Ball.Radius) - (Ball.PositionX - (GameField.Width - Ball.Radius));
                    Ball.DirectionX = Ball.DirectionX * -1;
                }

            }

            if (Ball.PositionY >= GameField.Height - Ball.Radius)
            {
                Ball.PositionY = (GameField.Height - Ball.Radius) - (Ball.PositionY - (GameField.Height - Ball.Radius));
                Ball.DirectionY = Ball.DirectionY * -1;
            }
            if (Ball.PositionX <= Ball.Radius)
            {
                if (scoring)
                {
                    GameTeams[1].score++;
                    OnTeamScored();
                }

                else
                {
                    Ball.PositionX = Ball.PositionX * -1;
                    Ball.DirectionX = Ball.DirectionX * -1;
                }
            }

            if (Ball.PositionY <= Ball.Radius)
            {
                Ball.PositionY = Ball.Radius + (Ball.Radius - Ball.PositionY);
                Ball.DirectionY = Ball.DirectionY * -1;
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
            foreach (KeyValuePair<int, GameTeam> Team in GameTeams)
            {
                foreach (Player player in Team.Value.PlayerList)
                {
                    if ( (Ball.LastTouchedTeam == player.Team) && !friendlyFire)
                        continue;
                    if (BallInPlayerBar(player))
                    {
                        Ball.LastTouchedTeam = player.Team;
                        float Angle = GetNewAngleOfBall(player);
                        ChangeDirectionOfBall(Angle);
                        return; // ASSUMING ONLY ONE PLAYER CAN TOUCH THE BALL !
                    }

                }
            }
        }
        
        public Rectangle RectangleOfPlayer(Player player)
        {
            return new Rectangle(player.PositionX, player.PositionY, player.Height, player.Width);
        }

        public Circle CircleOfBall(Ball ball)
        {
            return new Circle(ball.PositionX, ball.PositionY, ball.Radius);
        }

        private float GetNewAngleOfBall(Player p)
        {
            return GameInitializers.MAXIMUM_ANGLE_RAD * GetRelativeDistanceFromMiddlePoint(p);
        }

        private float GetRelativeDistanceFromMiddlePoint(Player p)
        {
            float relativeDistance = (Ball.PositionY - (p.PositionY + p.Height/2) ) / (p.Height/2);
            // temporary fix for corner special case 
            if (relativeDistance < -1)
                relativeDistance = -1;
            if (relativeDistance > 1)
                relativeDistance = 1;
            // ----------------------------------------
            return relativeDistance;
        }

        private void ChangeDirectionOfBall(float Angle)
        {
            if (Ball.DirectionX > 0)
            {
                ChangeAngle(Angle);
                Ball.DirectionX *= -1;
            }
            else
            {
                ChangeAngle(Angle);
            }

        }

        private void ChangeAngle(float Angle)
        {
            float speed = Ball.Speed;
            Ball.DirectionX = (float)Math.Cos(Angle) * speed;
            Ball.DirectionY = (float)Math.Sin(Angle) * speed;
        }

        /*
        private bool CircleInRectangle(Player p)
        {
            float minimumDistanceForCollision = (float)Math.Sqrt((Math.Pow(p.Height / 2, 2) + Math.Pow(p.Width / 2, 2))) + Ball.Radius;

            Tuple<float, float> PlayerBallYPositionTuple = Tuple.Create(Ball.PositionY, p.PositionY);
            float yDistance = Max(PlayerBallYPositionTuple) - Min(PlayerBallYPositionTuple);

            Tuple<float, float> PlayerBallXPositionTuple = Tuple.Create(Ball.PositionX, p.PositionX);
            float xDistance = Max(PlayerBallYPositionTuple) - Min(PlayerBallYPositionTuple);

            float actualDistance = (float)Math.Sqrt(Math.Pow(yDistance,2) + Math.Pow(xDistance,2 ));

            if (actualDistance > minimumDistanceForCollision)
                return false;                 
            
            if (actualDistance <= Ball.Radius)
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
                    if (Ball.PositionX >= p.PositionX)
                    {
                        Ball.DirectionX = Ball.DirectionX * -1;
                        return true;
                    }
                        
                }
                if (p.PositionX <= GameInitializers.BORDER_WIDTH / 2)
                {
                    if (Ball.PositionX <= p.PositionX)
                    {
                        Ball.DirectionX = Ball.DirectionX * -1;
                        return true;
                    }

                }
            }
                
            
            return false;
        }
        */
        
        private float Max(Tuple<float,float> floatTuple)
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

            if (TeamScored != null)
                TeamScored(this, EventArgs.Empty);
        }

        private void ResetBall()
        {
            Ball.PositionX = GameInitializers.BALL_POSX;
            Ball.PositionY = GameInitializers.BALL_POSY;
            Ball.DirectionX = GameInitializers.BALL_DIRX;
            Ball.DirectionY = GameInitializers.BALL_DIRY;
            Ball.LastTouchedTeam = -1;

            Thread.Sleep(1000); // After score give the Player some Time to relax
        }

        private bool PointInRectangular(Tuple<float, float> point, Rectangle rectangle)
        {
            bool betweenXLine = (rectangle.PositionX <= point.Item1) && (point.Item1 <= rectangle.PositionX + rectangle.Width);
            bool betweenYLine = (rectangle.PositionY <= point.Item2) && (point.Item2 <= rectangle.PositionY + rectangle.Height);
            return betweenXLine && betweenYLine;
        }

        private bool BallInPlayerBar(Player player)
        {
            Circle circle = new Circle(Ball.PositionX, Ball.PositionY, Ball.Radius);
            Rectangle rectangle = new Rectangle(player.PositionX, player.PositionY, player.Height, player.Width);
            return CircleInRect(circle, rectangle);
        }

        private bool CircleInRect(Circle circle, Rectangle rectangle)
        {
            Tuple<float, float> lefmost = Tuple.Create<float, float>(circle.PositionX - circle.Radius, circle.PositionY);
            Tuple<float, float> rightmost = Tuple.Create<float, float>(circle.PositionX + circle.Radius, circle.PositionY);

            Tuple<float, float> topmost = Tuple.Create<float, float>(circle.PositionX, circle.PositionY - circle.Radius);
            Tuple<float, float> bottommost = Tuple.Create<float, float>(circle.PositionX, circle.PositionY + circle.Radius);

            List<Tuple<float, float>> cornerPoints = new List<Tuple<float, float>>();
            cornerPoints.Add(lefmost);
            cornerPoints.Add(rightmost);
            cornerPoints.Add(topmost);
            cornerPoints.Add(bottommost);

            foreach (Tuple<float, float> point in cornerPoints)
            {
                if (PointInRectangular(point, rectangle))
                    return true;
            }

            return false;
        }


        public void AddPlayer(Player Player, int Team)
        {
            try
            {
                GameTeams[Team].PlayerList.Add(Player);
            }
            catch(KeyNotFoundException)
            {
                GameTeams.Add(Team, new GameTeam());
                GameTeams[Team].PlayerList.Add(Player);
            }                            
        }

        public int GetFreeTeam()
        {
            foreach (KeyValuePair<int, GameTeam> Team in GameTeams)
            {
                if (Team.Value.PlayerList.Count < maxPlayers / TEAM_COUNT)
                    return Team.Key;
            }
            return 0;
        }

        public Player[] GetAllPlayers()
        {
            List<Player> allPlayers = new List<Player>();

            foreach (KeyValuePair<int, GameTeam> entry in GameTeams)
            {
                foreach (Player player in entry.Value.PlayerList)
                    allPlayers.Add(player);
            }

            return allPlayers.ToArray();
        }

        public class Circle
        {
            public float PositionX;
            public float PositionY;
            public float Radius;

            public Circle(float PositionX, float PositionY, float Radius)
            {
                this.PositionX = PositionX;
                this.PositionY = PositionY;
                this.Radius = Radius;
            }
        }

        public class Rectangle
        {
            public float PositionX;
            public float PositionY;
            public float Height;
            public float Width;

            public Rectangle(float PositionX, float PositionY, float Height, float Width)
            {
                this.PositionX = PositionX;
                this.PositionY = PositionY;
                this.Height = Height;
                this.Width = Width;

            }
        }
        
        public class Point
        {
            public float PositionX;
            public float PositionY;
            
            public Point(float PositionX, float PositionY)
            {
                this.PositionX = PositionX;
                this.PositionY = PositionY;
            }
        }

        public class GameTeam
        {
            public int score;
            public List<Player> PlayerList;

            public GameTeam()
            {
                score = 0;
                PlayerList = new List<Player>();
            }
        }

    }
}
