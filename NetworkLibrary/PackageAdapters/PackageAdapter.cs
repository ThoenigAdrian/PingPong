﻿using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.DataPackages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace NetworkLibrary.PackageAdapters
{
    public class InvalidPackageException : Exception { };

    public class PackageAdapter
    {
        public PackageInterface CreatePackageFromNetworkData(byte[] data)
        {
            if (data == null)
                return null;

            string networkDataString = ConvertNetworkDataToString(data);

            PackageType type = GetPackageType(networkDataString);

            switch (type)
            {
                case PackageType.ServerData:
                    return JsonConvert.DeserializeObject<ServerDataPackage>(networkDataString);
                case PackageType.ClientControl:
                    return JsonConvert.DeserializeObject<ClientControlPackage>(networkDataString);
                case PackageType.ClientAddRequest:
                    return JsonConvert.DeserializeObject<ClientAddPlayerRequest>(networkDataString);
                case PackageType.ClientPlayerMovement:
                    return JsonConvert.DeserializeObject<PlayerMovementPackage>(networkDataString);
            }

            return null;   
        }

        public byte[] CreateNetworkDataFromPackage(PackageInterface package)
        {
            string networkDataString = JsonConvert.SerializeObject(package);
            return System.Text.Encoding.Default.GetBytes(networkDataString);
        }

        public PackageType GetPackageType(string json)
        {
            string PackageType = Newtonsoft.Json.Linq.JObject.Parse(json)["PackageType"].ToString();
            return (PackageType)Enum.Parse(typeof(PackageType), PackageType);
        }

        private string ConvertNetworkDataToString(byte[] array)
        {
            return System.Text.Encoding.Default.GetString(array);
        }

        
    
    }

}
