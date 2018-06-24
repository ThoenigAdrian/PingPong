using System.Collections.Generic;

namespace GameLogicLibrary.GameObjects
{
    // these are the only things i need to set and calculate in the client
    // everything else should be in a heirit of this class

    public class BasicStructure
    {
        public struct Score
        {
            public int Score_Team1;
            public int Score_Team2;
        }

        public Score _score;
        public GameField Field;
        public Ball Ball;
        public List<Player> Players;

        public BasicStructure(GameField field, Ball ball)
        {
            Field = field;
            Ball = ball;
            Players = new List<Player>();
            _score.Score_Team1 = 0;
            _score.Score_Team2 = 0;
        }

        public void CalculateFrame(int passedMicroSeconds)
        {

        }
    }
}
