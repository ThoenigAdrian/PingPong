using System.Collections.Generic;
using System;

namespace GameLogicLibrary.GameObjects
{
    public class GameStructure
    {
        public GameField GameField;
        public PlayBall Ball;
        public Dictionary<int,List<Player>> GameTeams = new Dictionary<int, List<Player>>();
        public int numberOfTeams = 2; // restrict to two teams for now
        public int maxPlayers { get; set; }

        public int PlayersCount
        {
            get
            {
                int PlayersCount = 0;

                foreach(KeyValuePair<int, List<Player>> entry in GameTeams)
                    PlayersCount += entry.Value.Count;
                
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

        public GameStructure()
        {
            this.maxPlayers = 2;
            GameField = new GameField();
            GameTeams.Add(0, new List<Player>());
            GameTeams.Add(1, new List<Player>());
            Ball = new PlayBall();
        }

        public GameStructure(int maxPlayers)
        {
            this.maxPlayers = maxPlayers;
            GameField = new GameField();
            GameTeams.Add(0, new List<Player>());
            GameTeams.Add(1, new List<Player>());
            Ball = new PlayBall();
        }

        public void CalculateFrame(long timePassedInMilliseconds)
        {
            float oldBallPositionX = Ball.PositionX;
            float oldBallPositionY = Ball.PositionY;

            Ball.PositionX += Ball.DirectionX * timePassedInMilliseconds;
            Ball.PositionY += Ball.DirectionY * timePassedInMilliseconds;

            foreach(KeyValuePair<int, List<Player>> Team in GameTeams)
            {
                foreach(Player player in Team.Value)
                {
                    if (player.PlayerBar.PositionY >= GameInitializers.BORDER_HEIGHT - player.PlayerBar.Height / 2)
                        player.PlayerBar.PositionY = GameInitializers.BORDER_HEIGHT - player.PlayerBar.Height / 2;
                    if (player.PlayerBar.PositionY <= 0)
                        player.PlayerBar.PositionY = 0;
                }
            }

            if (Ball.PositionX > GameField.Width)
            {
                Ball.PositionX = GameField.Width - (Ball.PositionX - GameField.Width);
                Ball.DirectionX = Ball.DirectionX * -1;
            }
                
            if (Ball.PositionY > GameField.Height)
            {
                Ball.PositionY = GameField.Height - (Ball.PositionY - GameField.Height);
                Ball.DirectionY = Ball.DirectionY * -1;
            }
            if (Ball.PositionX < 0)
            {
                Ball.PositionX = Ball.PositionX * -1;
                Ball.DirectionX = Ball.DirectionX * -1;
            }

            if (Ball.PositionY < 0)
            {
                Ball.PositionY = Ball.PositionY * -1;
                Ball.DirectionY = Ball.DirectionY * -1;
            }

            DetectCollisions();
        }

        private void DetectCollisions()
        {
            foreach (KeyValuePair<int, List<Player>> t in GameTeams)
            {
                foreach (Player p in t.Value)
                {
                    if (CircleInRectangular(p))
                    {
                        Ball.DirectionX = Ball.DirectionX * -1;
                        return; // assuming only one player can touch the ball
                    }
                        
                }
            }
        }

        private bool CircleInRectangular(Player p)
        {
            float minimumDistanceForCollision = (float)Math.Sqrt((Math.Pow(p.PlayerBar.Height / 2, 2) + Math.Pow(p.PlayerBar.Width / 2, 2))) + Ball.Radius;

            Tuple<float, float> PlayerBallYPositionTuple = Tuple.Create(Ball.PositionY, p.PlayerBar.PositionY);
            float yDistance = Max(PlayerBallYPositionTuple) - Min(PlayerBallYPositionTuple);

            Tuple<float, float> PlayerBallXPositionTuple = Tuple.Create(Ball.PositionX, p.PlayerBar.PositionX);
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
                if (p.PlayerBar.PositionX >= GameInitializers.BORDER_WIDTH / 2)
                {
                    if (Ball.PositionX >= p.PlayerBar.PositionX)
                    {
                        Ball.DirectionX = Ball.DirectionX * -1;
                        return true;
                    }
                        
                }
                if (p.PlayerBar.PositionX <= GameInitializers.BORDER_WIDTH / 2)
                {
                    if (Ball.PositionX <= p.PlayerBar.PositionX)
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

        public void AddPlayer(Player Player, int Team)
        {
            try
            {
                GameTeams[Team].Add(Player);
            }
            catch(KeyNotFoundException)
            {
                GameTeams.Add(Team, new List<Player>());
                GameTeams[Team].Add(Player);
            }                            
        }

        public int GetFreeTeam()
        {
            foreach (KeyValuePair<int, List<Player>> entry in GameTeams)
            {
                if (entry.Value.Count < maxPlayers / numberOfTeams)
                    return entry.Key;
            }
            return 0;
        }

        public Player[] GetAllPlayers()
        {
            List<Player> allPlayers = new List<Player>();

            foreach (KeyValuePair<int, List<Player>> entry in GameTeams)
            {
                foreach (Player player in entry.Value)
                    allPlayers.Add(player);
            }

            return allPlayers.ToArray();
        }

    }
}
