using NetworkLibrary.DataStructs;
using Newtonsoft;
using System;

namespace NetworkLibrary.PackageAdapters
{
    public class InvalidPackageException : Exception { };
    public class Package
    {
        public object CreatePackageFromNetworkData(byte[] data)
        {
            string networkDataString = ConvertNetworkDataToString(data);

            if (GetPackageType(networkDataString) == PackageType.ServerData)
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ServerDataPackage>(networkDataString);
            if (GetPackageType(networkDataString) == PackageType.ClientControl)
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ClientControlPackage>(networkDataString);
            else
                throw new InvalidPackageException();
        
        }

        public byte[] CreateNetworkDataFromPackage(object package)
        {
            string networkDataString = Newtonsoft.Json.JsonConvert.SerializeObject(package);
            return System.Text.Encoding.Default.GetBytes(networkDataString);
        }

        private string ConvertNetworkDataToString(byte[] array)
        {
            return System.Text.Encoding.Default.GetString(array);
        }

        private PackageType GetPackageType(string json)
        {
            string PackageType = Newtonsoft.Json.Linq.JObject.Parse(json)["PackageType"].ToString();
            return (PackageType) Enum.Parse(typeof(PackageType), PackageType);
        }
    
    }

}
