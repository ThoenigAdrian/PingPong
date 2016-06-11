using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    class ClientInitializeGamePackage : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientInitalizeGamePackage; } }

    }
}
