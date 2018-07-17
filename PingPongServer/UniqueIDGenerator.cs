using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingPongServer
{
    public class UniqueIDGenerator
    {
        // Will create random unique IDs(32 bit integer) 
        // To Free a ID make sure to call FreeSessionID
        // Multiple Instances of this class can return the same random values if they have been created at the same time. So don't use this for cryptographically secure stuff.
        private List<int> UsedIDs = new List<int>();
        object ListModificationOngoing = new object();
        private static readonly Random RandomNumberGenerator = new Random();
        public UniqueIDGenerator()
        {
        }
        public int GetSessionID()
        {
            int RandomSessionID = RandomNumberGenerator.Next();
            lock (ListModificationOngoing)
            {
                while (UsedIDs.Contains(RandomSessionID))
                {
                    RandomSessionID = RandomNumberGenerator.Next();
                }
                UsedIDs.Add(RandomSessionID);
            }
            return RandomSessionID;

        }
        public void FreeSessionID(int SessionID)
        {
            lock (ListModificationOngoing)
            {
                UsedIDs.Remove(SessionID);
            }
        }
    }
}
