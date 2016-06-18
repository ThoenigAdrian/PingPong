using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    public class ClientSessionReconnect : ClientRegisteredPackage
    {
        public override PackageType PackageType
        {
            get { return PackageType.ClientControl; }
        }

        public int sessionID;
    }
}
