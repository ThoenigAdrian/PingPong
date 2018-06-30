using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.DataPackages.ServerSourcePackages.Matchmaking
{
    public class ServerGameInValidResponse : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerGameInvalidResponse; }
        }
        
        public string ErrorMessage;
    }
}
