using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLogicLibrary.GameObjects;

namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    public class ClientInitializeGamePackage : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientInitalizeGamePackage; } }
        public int PlayerCount { get; set; }

    }
}
