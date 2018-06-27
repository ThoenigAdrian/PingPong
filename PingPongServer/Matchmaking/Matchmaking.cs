using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Collections.Generic;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        Dictionary<int, MatchingEntry> m_waitingForMatch = new Dictionary<int, MatchingEntry>();

        public bool AddRequestToQueue(NetworkConnection connection, int maxPlayerCount, int[] teamWishes)
        {
            ClientData request = new ClientData()
            {
                ClientConnection = connection,
                MaxPlayerCount = maxPlayerCount,
                TeamWishes = teamWishes
            };

            if (!IsRequestValid(request))
                return false;

            AddSearchingClient(request);
            return true;
        }
        
        private void AddSearchingClient(ClientData client)
        {
            int maxPlayerCount = client.MaxPlayerCount;

            MatchingEntry sameSizedEntry = null;

            if (GameSizeOpen(maxPlayerCount))
            {
                sameSizedEntry = m_waitingForMatch[maxPlayerCount];
            }
            else
            {
                sameSizedEntry = new MatchingEntry();
                sameSizedEntry.Game = new Game() { MaxPlayerCount = maxPlayerCount };
                m_waitingForMatch.Add(maxPlayerCount, sameSizedEntry);
            }

            sameSizedEntry.GroupedClients.AddRequest(client);
        } 

        private bool GameSizeOpen(int maxPlayerCount)
        {
            return m_waitingForMatch.ContainsKey(maxPlayerCount);
        } 

        public void FindMatch()
        {
            ClientData client1 = new ClientData()
            {
                MaxPlayerCount = 6,
                TeamWishes = new int[2] { 0, 1 }
            };

            ClientData client2 = new ClientData()
            {
                MaxPlayerCount = 6,
                TeamWishes = new int[4] { 0, 1 ,1, 0}
            };

            Game testGame = new Game();
            testGame.MaxPlayerCount = client1.MaxPlayerCount;
            testGame.AddClient(client1);
            testGame.AddClient(client2);

            bool fits = testGame.FitsIntoGame();
            bool full = testGame.Full();

            //foreach(MatchingEntry entry in m_waitingForMatch.Values)
            //{
            //    HandleEntry(entry);
            //}
        }

        private void HandleEntry(MatchingEntry entry)
        {
            //forea
        }

        bool IsRequestValid(ClientData request)
        {
            if (request.MaxPlayerCount % 2 != 0)
                return false;

            if (request.ClientPlayerCount > request.MaxPlayerCount)
                return false;

            if (request.Team1Count > request.TeamSize || request.Team2Count > request.TeamSize)
                return false;

            return true;
        }
    }
}
