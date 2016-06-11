using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerJoinGameResponsePackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerData; }
        }

        public Result Result;
        public string Reason;
    }
}
