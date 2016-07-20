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
            float oldBallPositionX = Ball.PositionX;
            float oldBallPositionY = Ball.PositionY;

            Ball.PositionX += Ball.DirectionX * timePassedInMilliseconds;
            Ball.PositionY += Ball.DirectionY * timePassedInMilliseconds;

            foreach(KeyValuePair<int, GameTeam> Team in GameTeams)
            {
                foreach(Player player in Team.Value.PlayerList)
                {
                    player.PositionY += player.DirectionY;

                    if (player.PositionY >= GameInitializers.BORDER_HEIGHT - player.Height)
                        player.PositionY = GameInitializers.BORDER_HEIGHT - player.Height;
                    if (player.PositionY <= 0)
                        player.PositionY = 0;
                }
            }

            if (Ball.PositionX >= GameField.Width - Ball.Radius)
            {
                GameTeams[0].score++;
                OnTeamScored();
                // Uncomment this Line if you wan't to allow Back Windows To Reflect aka. no scoring
                //Ball.PositionX = (GameField.Width - Ball.Radius) - (Ball.PositionX - (GameField.Width - Ball.Radius));
                //Ball.DirectionX = Ball.DirectionX * -1;
            }
                
            if (Ball.PositionY >= GameField.Height - Ball.Radius)
            {
                Ball.PositionY = (GameField.Height - Ball.Radius) - (Ball.PositionY - (GameField.Height - Ball.Radius));
                Ball.DirectionY = Ball.DirectionY * -1;
            }
            if (Ball.PositionX <= Ball.Radius)
            {
                GameTeams[1].score++;
                OnTeamScored();
                // Uncomment this Line if you wan't to allow Back Windows To Reflect aka. no scoring
                // Ball.PositionX = (GameField.Width - Ball.Radius) - (Ball.PositionX - (GameField.Width - Ball.Radius));
                // Ball.PositionX = Ball.PositionX * -1;
                // Ball.DirectionX = Ball.DirectionX * -1;
            }

            if (Ball.PositionY <= Ball.Radius)
            {
                Ball.PositionY = Ball.Radius + (Ball.Radius - Ball.PositionY);
                Ball.DirectionY = Ball.DirectionY * -1;
            }

            DetectCollisions();
        }

        private void DetectCollisions()
        {
            foreach (KeyValuePair<int, GameTeam> Team in GameTeams)
            {
                foreach (Player p in Team.Value.PlayerList)
                {
                    if (Ball.LastTouchedTeam == p.Team && !friendlyFire)
                        continue;
                    if (CircleInRect(p, Ball))
                    {
                        Ball.LastTouchedTeam = p.Team;
                        float Angle = GetNewAngle(p);
                        ChangeDirection(Angle);
                        return; // assuming only one player can touch the ball
                    }
                        
                }
            }
        }

        private float GetNewAngle(Player p)
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

        private void ChangeDirection(float Angle)
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
                

            //if (Ball.PositionX > GameInitializers.BORDER_WIDTH / 2)
            //    Ball.DirectionX *= -1;

        }

        private void ChangeAngle(float Angle)
        {
            float speed = Ball.Speed;
            Ball.DirectionX = (float)Math.Cos(Angle) * speed;
            Ball.DirectionY = (float)Math.Sin(Angle) * speed;
        }

        private bool CircleInRectangular(Player p)
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
        
        private float Max(Tuple<float,float> t)
        {
            return Math.Max(t.Item1, t.Item2);
        }

        private float Min(Tuple<float, float> t)
        {
            return Math.Min(t.Item1, t.Item2);
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

        private bool PointInRectangular(Tuple<float, float> point, Player pb)
        {
            bool betweenXLine = (pb.PositionX <= point.Item1) && (point.Item1 <= pb.PositionX + pb.Width);
            bool betweenYLine = (pb.PositionY <= point.Item2) && (point.Item2 <= pb.PositionY + pb.Height);
            return betweenXLine && betweenYLine;
        }
        private bool CircleInRect(Player p, Ball b)
        {
            Tuple<float, float> lefmost = Tuple.Create<float, float>(b.PositionX - b.Radius, b.PositionY);
            Tuple<float, float> rightmost = Tuple.Create<float, float>(b.PositionX + b.Radius, b.PositionY);

            Tuple<float, float> topmost = Tuple.Create<float, float>(b.PositionX, b.PositionY - b.Radius);
            Tuple<float, float> bottommost = Tuple.Create<float, float>(b.PositionX, b.PositionY + b.Radius);

            List<Tuple<float, float>> list = new List<Tuple<float, float>>();
            list.Add(lefmost);
            list.Add(rightmost);
            list.Add(topmost);
            list.Add(bottommost);

            foreach (Tuple<float, float> point in list)
            {
                if (PointInRectangular(point, p))
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
