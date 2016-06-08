using NetworkLibrary.DataStructs;
using Newtonsoft;
using System;

namespace NetworkLibrary.PackageAdapters
{
    public abstract class PackageAdapterInterface
    {
        public abstract PackageInterface ByteToPackage(byte[] data);
        public abstract byte[] PackageToByte(PackageInterface package);

        protected PackageType GetPackageType(string json)
        {
            string PackageType = Newtonsoft.Json.Linq.JObject.Parse(json)["PackageType"].ToString();
            return (PackageType) Enum.Parse(typeof(PackageType), PackageType);
        }
    
    }

}
