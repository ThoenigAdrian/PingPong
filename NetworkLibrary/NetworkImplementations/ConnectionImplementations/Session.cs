using System;

namespace NetworkLibrary.NetworkImplementations.ConnectionImplementations
{
    public class Session
    {
        public int SessionID { get; set; }

        public Session()
        {
            SessionID = new Random().Next();
        }

        public Session(int ID)
        {
            SessionID = ID;
        }
    }
}
