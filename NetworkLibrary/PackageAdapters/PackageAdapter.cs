using NetworkLibrary.DataPackages;
using System;

namespace NetworkLibrary.PackageAdapters
{
    public class InvalidPackageException : Exception { };

    public abstract class PackageAdapter
    {
        public abstract PackageInterface[] CreatePackagesFromStream(byte[] stream);

        public abstract PackageInterface CreatePackageFromNetworkData(byte[] data);

        public abstract byte[] CreateNetworkDataFromPackage(PackageInterface package);

        public abstract PackageType GetPackageType(string json);
    }
}
