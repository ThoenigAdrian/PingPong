using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.DataPackages;
using Newtonsoft.Json;
using System;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using System.Collections.Generic;

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
