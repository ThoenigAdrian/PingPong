using NetworkLibrary.DataStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.PackageAdapters
{
    abstract class PackageAdapterInterface
    {
        public abstract PackageInterface ByteToPackage(byte[] data);
        public abstract byte[] PackageToByte(PackageInterface package);
    }
}
