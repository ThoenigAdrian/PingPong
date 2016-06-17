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
        }

        private void CollisionDetection()
        {
            foreach (KeyValuePair<int, List<Player>> t in GameTeams)
            {
                foreach (Player p in t.Value)
                {
                    CircleInRectangular(p);
                }
            }
        }

        private bool CircleInRectangular(Player p)
        {
            /*
            Tuple<float,float> a = Tuple.Create(Ball.PositionY, p.PlayerBar.PositionY);
            if (Max(a) - Min(a) <= Ball.Radius)
                return true; // this is a special case the return true has to be substituted by a deeper looking algorithm which checks if those two are colliding
            if (Ball.PositionY + Ball.Radius --- Ball.PositionY - Ball.Radius p.PlayerBar.Height + p.PlayerBar.PositionY --- p.PlayerBar.Height - p.PlayerBar.PositionY)
            Ball.PositionY + Ball.Radius (Ball.PositionY - Ball.Radius) ||*/
            return false; //will be implemented later
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
