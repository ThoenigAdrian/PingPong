﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    public class ClientJoinGameRequest : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientPlayerMovement; } }
    }
}