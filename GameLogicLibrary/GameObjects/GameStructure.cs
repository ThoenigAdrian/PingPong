using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogicLibrary.GameObjects
{
    public class GameStructure
    {
        public GameField m_field;
        public PlayBall m_ball;
        public PlayerBar m_player1;
        public PlayerBar m_player2;

        public GameStructure()
        {
            m_field = new GameField();
            m_ball = new PlayBall();
            m_player1 = new PlayerBar(GameInitializers.PLAYER_1_X);
            m_player2 = new PlayerBar(GameInitializers.PLAYER_2_X);
        }
    }
}
