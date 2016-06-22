using System.Collections.Generic;
using GameLogicLibrary.GameObjects;

namespace PingPongServer.ServerGame.ServerGameObjects
{
    public class Client
    {
        
        public int ClientID;
        public int session;
        public List<Player> Players = new List<Player>();

        public Client(GameStructure gameStructure, int sessionID)
        {
            this.session = sessionID;
        }

        public void AddPlayer(Player player, GameStructure GameStructure)
        {
            Players.Add(player);
            GameStructure.AddPlayer(player, GameStructure.GetFreeTeam());
        }

    }
}
