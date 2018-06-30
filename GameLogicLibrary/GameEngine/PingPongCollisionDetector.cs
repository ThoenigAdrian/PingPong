using System;
using System.Collections.Generic;
using GameLogicLibrary;
using GameLogicLibrary.GameObjects;
using System.Linq;
using System.Text;

namespace GameLogicLibrary
{
    
    public class PingPongCollisionDetector
    {
        private CollisionDetector CollisionDetector;
        private GameStructure GameStructure;
        

        public PingPongCollisionDetector(GameStructure GameStructure)
        {
            this.GameStructure = GameStructure;
            CollisionDetector = new CollisionDetector();
        }

        public void CalculateCollisions(TeamScoredDelegateClass OnTeamScored)
        {
            CalculatePlayerBallCollisions();
            CalculateWallCollisions(OnTeamScored);
        }


        private void CalculateWallCollisions(TeamScoredDelegateClass OnTeamScored)
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
                    GameStructure.Ball.PositionX = GameStructure.Ball.PositionX * -1;
                    GameStructure.Ball.ReverseDirectionX();
                }
            }

            if (GameStructure.Ball.PositionY <= GameStructure.Ball.Radius)
            {
                GameStructure.Ball.PositionY = GameStructure.Ball.Radius + (GameStructure.Ball.Radius - GameStructure.Ball.PositionY);
                GameStructure.Ball.ReverseDirectionY();
            }

        }

        public void CalculatePlayerBallCollisions()
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
                        ApplyPlayerBallCollision(Angle);
                        return; // ASSUMING ONLY ONE PLAYER CAN TOUCH THE BALL !
                    }

                }
            }
        }

        private void ApplyPlayerBallCollision(float Angle)
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

        private float GetRelativeDistanceFromMiddlePoint(Player p)
        {
            float relativeDistance = (GameStructure.Ball.PositionY - (p.PositionY + p.Height / 2)) / (p.Height / 2);

            if (relativeDistance < -1)
                relativeDistance = -1;
            if (relativeDistance > 1)
                relativeDistance = 1;

            return relativeDistance;
        }

        private float GetNewAngleOfBall(Player p)
        {
            return GameInitializers.MAXIMUM_ANGLE_RAD * GetRelativeDistanceFromMiddlePoint(p);
        }

        private bool BallInPlayerBar(Player player)
        {
            GameStructure.Circle circle = new GameStructure.Circle(GameStructure.Ball.PositionX, GameStructure.Ball.PositionY, GameStructure.Ball.Radius);
            GameStructure.Rectangle rectangle = new GameStructure.Rectangle(player.PositionX, player.PositionY, player.Height, player.Width);
            return CollisionDetector.CircleInRect(circle, rectangle);
        }
    }
}
