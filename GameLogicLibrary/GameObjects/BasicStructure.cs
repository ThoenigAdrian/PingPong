using System.Collections.Generic;

namespace GameLogicLibrary.GameObjects
{
    // these are the only things i need to set and calculate in the client
    // everything else should be in a heirit of this class

    public class BasicStructure
    {
        public GameField Field;
        public Ball Ball;
        public List<Player> Players;

        public BasicStructure(GameField field, Ball ball)
        {
            Field = field;
            Ball = ball;
            Players = new List<Player>();
        }

        public void CalculateFrame(int passedMicroSeconds)
        {

        }
    }
}
