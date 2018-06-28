using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using System.Collections.Generic;
using System.Net.Sockets;
using XSLibrary.Network.Connections;

namespace PingPongServer
{
    public partial class Matchmaking
    {
        Dictionary<int, MatchingEntry> m_waitingForMatch = new Dictionary<int, MatchingEntry>();

        public bool AddRequestToQueue(NetworkConnection connection, int maxPlayerCount, int[] teamWishes)
        {
            Request request = new Request(/*connection,*/ maxPlayerCount, teamWishes);

            if (!IsRequestValid(request))
                return false;

            AddSearchingClient(request);
            return true;
        }
        
        private void AddSearchingClient(Request client)
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

            sameSizedEntry.GroupedRequests.AddRequest(client);
        } 

        private bool GameSizeOpen(int maxPlayerCount)
        {
            return m_waitingForMatch.ContainsKey(maxPlayerCount);
        } 

        public void FindMatch()
        {
            int maxPlayerCount = 8;

            Request client1 = new Request(maxPlayerCount, new int[2] { 0, 0 });
            Request client2 = new Request(maxPlayerCount, new int[3] { 0, 0, 1 });
            Request client3 = new Request(maxPlayerCount, new int[3] { 0, 0, 0 });

            bool equal = client2.IsEqualRequest(client3);

            Game testGame = new Game();
            testGame.MaxPlayerCount = client1.MaxPlayerCount;
            testGame.AddRequest(client1);
            testGame.AddRequest(client2);
            testGame.AddRequest(client3);

            bool ready = testGame.GameReady();

            int[] final1 = client1.GetPlayerPlacements();
            int[] final2 = client2.GetPlayerPlacements();
            int[] final3 = client3.GetPlayerPlacements();


            //foreach(MatchingEntry entry in m_waitingForMatch.Values)
            //{
            //    HandleEntry(entry);
            //}
        }

        private void HandleEntry(MatchingEntry entry)
        {
            //forea
        }

        bool IsRequestValid(Request request)
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
