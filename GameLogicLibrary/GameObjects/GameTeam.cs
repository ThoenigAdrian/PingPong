using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogicLibrary.GameObjects
{
    public class GameTeam
    {
        public Teams Team;
        public List<Player> PlayersOfTeam = new List<Player>();
        public int PlayerCount { get { return PlayersOfTeam.Count; } set { } }

        public GameTeam(Teams Team)
        {

            this.Team = Team;
        }

        public void AddPlayer(Player playerToAdd)
        {
            PlayersOfTeam.Add(playerToAdd);

        }
    }
}
