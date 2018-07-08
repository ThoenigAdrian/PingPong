using System;
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
        public event GameFinishedEventHandler OnGameFinished;
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

        public void PauseBall(int millisecs)
        {
            GameOngoingTimer = new OneShotTimer(millisecs * 1000, true);
        }

        public void SetPlayerMovement(Player player, ClientMovement LastPlayerMovement)
        {
            if (LastPlayerMovement == ClientMovement.Down)
                player.DirectionY = player.Speed;
            else if (LastPlayerMovement == ClientMovement.Up)
                player.DirectionY = -player.Speed;
            else if (LastPlayerMovement == ClientMovement.StopMoving)
                player.DirectionY = 0;
        }

        private void ResetBall()
        {
            GameStructure.Ball.PositionX = GameInitializers.BALL_POSX;
            GameStructure.Ball.PositionY = GameInitializers.BALL_POSY;
            RandomizeBallDirection();
            GameStructure.Ball.ResetBall();
            PauseBall(1000);
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
                    OnGameFinished?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void IncreaseBallSpeed()
        {
            GameStructure.Ball.IncreaseSpeed(GameOngoingTimer.TimePassed);
        }

        private float Max(Tuple<float, float> floatTuple)
        {
            return Math.Max(floatTuple.Item1, floatTuple.Item2);
        }

        private float Min(Tuple<float, float> floatTuple)
        {
            return Math.Min(floatTuple.Item1, floatTuple.Item2);
        }

    }
}
