﻿using NetworkLibrary.DataStructs;
using System;

namespace NetworkLibrary.PackageAdapters
{
    class ServerTCPAdapter : PackageAdapterInterface
    {
        public override PackageInterface ByteToPackage(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] PackageToByte(PackageInterface package)
        {
            throw new NotImplementedException();
        }
    }
}