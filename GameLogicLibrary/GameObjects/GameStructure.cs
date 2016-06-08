using System.Collections.Generic;

namespace GameLogicLibrary.GameObjects
{
    public class GameStructure
    {
        public GameField m_field;
        public PlayBall m_ball;
        public List<PlayerBar> m_players;

        public GameStructure()
        {
            m_field = new GameField();
            m_ball = new PlayBall();
            m_players = new List<PlayerBar>();
            m_players.Add(new PlayerBar(GameInitializers.PLAYER_1_X));
            m_players.Add(new PlayerBar(GameInitializers.PLAYER_2_X));
        }
    }
}
