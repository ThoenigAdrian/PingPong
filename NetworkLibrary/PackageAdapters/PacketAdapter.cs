using NetworkLibrary.DataStructs;
using System;

namespace NetworkLibrary.PackageAdapters
{
    public class PacketAdapter : PackageAdapterInterface
    {
        public override PackageInterface ByteToPackage(byte[] data)
        {
            ServerDataPackage serverData = new ServerDataPackage();
            serverData.TestValue = data[0];
            return serverData;
        }

        public override byte[] PackageToByte(PackageInterface package)
        {
            ServerDataPackage data = package as ServerDataPackage;
            byte[] byteData = new byte[1] { Convert.ToByte(data.TestValue) };
            return byteData;
        }
    }
}
