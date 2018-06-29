using System.Collections.Generic;
using GameLogicLibrary.GameObjects;

namespace PingPongServer.ServerGame.ServerGameObjects
{
    public class Client
    {
        public int SessionID;
        public List<Player> Players = new List<Player>();

        public Client(GameStructure gameStructure, int sessionID)
        {
            this.SessionID = sessionID;
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public override string ToString()
        {
            string s = "";
            s += "\nClient ID: " + SessionID.ToString();
            s += "\nThis Client contains the following players :";
            foreach (Player p in Players)
                s += "\n\t" + p.ToString();
            return s;
        }

    }
}
