using System;
using System.Threading;
using System.Collections.Generic;
using GameLogicLibrary.GameObjects;
using XSLibrary.Utility;

namespace GameLogicLibrary
{

    public delegate void TeamScoredDelegateClass();

    public class GameEngine
    {
        private GameStructure GameStructure;

        public delegate void TeamScoredEventHandler(object sender, EventArgs e);
        public event TeamScoredEventHandler TeamScoredHandler;
        public delegate void GameFinishedEventHandler(object sender, EventArgs e);
        public event GameFinishedEventHandler GameFinishedHandler;
        private PingPongCollisionDetector Collisions;

        OneShotTimer GameOngoingTimer;
        
        TeamScoredDelegateClass TeamScoredDelegate;

        Random random;

        public GameEngine(GameStructure GameStructure)
        {
            this.GameStructure = GameStructure;
            Collisions = new PingPongCollisionDetector(GameStructure);
            TeamScoredDelegate = new TeamScoredDelegateClass(OnTeamScored);
            random = new Random();
            ResetBall();
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

            Collisions.CalculateCollisions(TeamScoredDelegate);
            IncreaseBallSpeed();
        }

        private void CalculatePlayerPosition(Player player)
        {
            player.PositionY += player.DirectionY;

            if (player.PositionY >= GameInitializers.BORDER_HEIGHT - player.Height)
                player.PositionY = GameInitializers.BORDER_HEIGHT - player.Height;
            if (player.PositionY <= 0)
                player.PositionY = 0;
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
            else if (angle >= 90 && angle < 135)
                angle += 45;
            else if (angle > 225 && angle < 270)
                angle -= 45;
            else if (angle >= 270 && angle < 315)
                angle += 45;

            float radiant = GameMath.DegreeToRadian(angle);
            GameStructure.Ball.ChangeAngleOfBall(radiant);
        }

        

        private void OnTeamScored()
        {
            ResetBall();
            TeamScoredHandler?.Invoke(this, EventArgs.Empty);
            CheckIfOneTeamWon();
        }

        private void CheckIfOneTeamWon()
        {
            for (int index = 0; index < GameStructure.GameTeams.Count; index++)
            {
                if (GameStructure.GameTeams[index].score >= GameInitializers.SCORE_NEEDED_FOR_VICTORY)
                {
                    GameFinishedHandler?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void IncreaseBallSpeed()
        {
            GameStructure.Ball.increaseSpeed(GameOngoingTimer.TimePassed);
        }

        private float Max(Tuple<float, float> floatTuple)
        {
            return Math.Max(floatTuple.Item1, floatTuple.Item2);
        }

        private float Min(Tuple<float, float> floatTuple)
        {
            return Math.Min(floatTuple.Item1, floatTuple.Item2);
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
