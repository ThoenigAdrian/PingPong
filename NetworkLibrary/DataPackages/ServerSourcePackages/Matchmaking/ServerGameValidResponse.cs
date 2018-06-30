using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.DataPackages.ServerSourcePackages.Matchmaking
{

    public class ServerGameValidResponse : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerGameValidResponse; }
        }
        
        public string Status;
        public bool WaitingForAdditionalPlayers = true;
    }

}
