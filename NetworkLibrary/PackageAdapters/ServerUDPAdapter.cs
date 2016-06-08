using NetworkLibrary.DataStructs;
using System;

namespace NetworkLibrary.PackageAdapters
{
    public class ServerUDPAdapter : PackageAdapterInterface
    {
        public override PackageInterface ByteToPackage(byte[] data)
        {
            ServerDataPackage package = new ServerDataPackage();
            byte[] buffer = new byte[4];

            Array.Copy(data, 0, buffer, 0, buffer.Length);
            package.BallPosX = Convert.ToInt32(buffer);

            Array.Copy(data, 4, buffer, 0, buffer.Length);
            package.BallPosY = Convert.ToInt32(buffer);

            return package;
        }

        public override byte[] PackageToByte(PackageInterface package)
        {
            ServerDataPackage serverPackage = package as ServerDataPackage;
 
            byte[] data = new byte[8];

            byte[] buffer = BitConverter.GetBytes(serverPackage.BallPosX);
            Array.Copy(buffer, 0, data, 0, buffer.Length);

            buffer = BitConverter.GetBytes(serverPackage.BallPosY);
            Array.Copy(buffer, 0, data, 4, buffer.Length);

            return data;
        }
    }
}
