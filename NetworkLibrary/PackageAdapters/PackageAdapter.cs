using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.DataPackages;
using Newtonsoft.Json;
using System;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using System.Collections.Generic;

namespace NetworkLibrary.PackageAdapters
{
    public class InvalidPackageException : Exception { };

    public class PackageAdapter
    {
        public PackageInterface[] CreatePackagesFromStream(byte[] stream)
        {
            if (stream == null)
                return null;

            byte[][] dataSets = SplitStreamIntoData(stream);

            if (dataSets == null)
                return null;

            List<PackageInterface> packages = new List<PackageInterface>();
            foreach (byte[] data in dataSets)
            {
                PackageInterface package = CreatePackageFromNetworkData(data);
                if (package != null)
                    packages.Add(package);
            }

            if (packages.Count > 0)
                return packages.ToArray();

            return null;
        }

        private byte[][] SplitStreamIntoData(byte[] stream)
        {
            if (stream == null)
                return null;

            List<byte[]> dataSets = new List<byte[]>();

            dataSets.Add(stream);

            return dataSets.ToArray();
        }

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
                case PackageType.ClientAddPlayerRequest:
                    return JsonConvert.DeserializeObject<ClientAddPlayerRequest>(networkDataString);
                case PackageType.ClientInitalizeGamePackage:
                    return JsonConvert.DeserializeObject<ClientInitializeGamePackage>(networkDataString);
                case PackageType.ClientJoinGameRequest:
                    return JsonConvert.DeserializeObject<ClientJoinGameRequest>(networkDataString);
                case PackageType.ServerAddPlayerResponsePackage:
                    return JsonConvert.DeserializeObject<ServerAddPlayerResponsePackage>(networkDataString);
                case PackageType.ServerGameControl:
                    return JsonConvert.DeserializeObject<ServerGameControlPackage>(networkDataString);
                case PackageType.ServerJoinGameResponsePackage:
                    return JsonConvert.DeserializeObject<ServerJoinGameResponsePackage>(networkDataString);
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

        private string[] ConvertStreamToValidJsonStrings(string json)
        {
            int curlyBracketsCount = 0;
            int squareBracketsCount = 0;
            int startIndex = 0;
            int endIndex = 0;

            if (json.Length == 0)
                return null;

            List<string> jsonStrings = new List<string>();

            foreach (char character in json)
            {
                if (character == '[')
                    squareBracketsCount++;
                if (character == ']')
                    squareBracketsCount--;
                if (character == '{')
                    curlyBracketsCount++;
                if (character == '}')
                    curlyBracketsCount--;

                endIndex++;

                if (curlyBracketsCount == 0 && squareBracketsCount == 0)
                {
                    jsonStrings.Add(json.Substring(startIndex, endIndex - startIndex));
                    startIndex = endIndex;
                }
            }

            return jsonStrings.ToArray();
        }

        private string ConvertNetworkDataToString(byte[] array)
        {
            return System.Text.Encoding.Default.GetString(array);
        }    
    }

}
