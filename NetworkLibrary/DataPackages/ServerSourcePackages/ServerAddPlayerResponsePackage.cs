using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerAddPlayerResponsePackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerData; }
        }

        public Result Result;
    }
}
