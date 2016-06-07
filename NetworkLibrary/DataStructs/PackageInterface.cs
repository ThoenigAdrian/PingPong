using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.DataStructs
{
    abstract class PackageInterface
    {
        public abstract PackageType PackageType { get; }
    }

    enum PackageType
    {
        ClientControl,
        ServerData
    }
}
