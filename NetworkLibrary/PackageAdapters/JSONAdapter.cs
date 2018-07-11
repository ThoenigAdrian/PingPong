using System.Collections.Generic;
using NetworkLibrary.DataPackages;
using Newtonsoft.Json;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using System;

namespace NetworkLibrary.PackageAdapters
{
    public class JSONAdapter : PackageAdapter
    { 
        public override PackageType GetPackageType(string json)
        {
            string PackageType = Newtonsoft.Json.Linq.JObject.Parse(json)["PackageType"].ToString();
            return (PackageType)Enum.Parse(typeof(PackageType), PackageType);
        }

        public override PackageInterface[] CreatePackagesFromStream(byte[] stream)
        {
            if (stream == null)
                return null;

            string[] jsonStrings = ConvertStreamToValidJsonStrings(ConvertNetworkDataToString(stream));

            if (jsonStrings == null)
                return null;

            List<PackageInterface> packages = new List<PackageInterface>();
            foreach (string data in jsonStrings)
            {
                PackageInterface package = CreatePackageFromJSONString(data);
                if (package != null)
                    packages.Add(package);
            }

            if (packages.Count > 0)
                return packages.ToArray();

            return null;
        }

        public override PackageInterface CreatePackageFromNetworkData(byte[] data)
        {
            if (data == null)
                return null;

            return CreatePackageFromJSONString(ConvertNetworkDataToString(data));
        }

        public override byte[] CreateNetworkDataFromPackage(PackageInterface package)
        {
            string networkDataString = JsonConvert.SerializeObject(package);
            return System.Text.Encoding.Default.GetBytes(networkDataString);
        }

        private PackageInterface CreatePackageFromJSONString(string jsonString)
        {
            try
            {
                PackageType type = GetPackageType(jsonString);

                switch (type)
                {
                    case PackageType.ServerData:
                        return JsonConvert.DeserializeObject<ServerDataPackage>(jsonString);
                    case PackageType.ClientControl:
                        return JsonConvert.DeserializeObject<ClientControlPackage>(jsonString);
                    case PackageType.OpenPort:
                        return JsonConvert.DeserializeObject<ClientOpenPortPackage>(jsonString);
                    case PackageType.ClientInitalizeGamePackage:
                        return JsonConvert.DeserializeObject<ClientInitializeGamePackage>(jsonString);
                    case PackageType.ServerGameControl:
                        return JsonConvert.DeserializeObject<ServerGameControlPackage>(jsonString);
                    case PackageType.ServerSessionResponse:
                        return JsonConvert.DeserializeObject<ServerSessionResponse>(jsonString);
                    case PackageType.ClientPlayerMovement:
                        return JsonConvert.DeserializeObject<PlayerMovementPackage>(jsonString);
                    case PackageType.ClientSessionRequest:
                        return JsonConvert.DeserializeObject<ClientSessionRequest>(jsonString);
                    case PackageType.ServerPlayerIDResponse:
                        return JsonConvert.DeserializeObject<ServerInitializeGameResponse>(jsonString);
                    case PackageType.ServerMatchmakingStatusResponse:
                        return JsonConvert.DeserializeObject<ServerMatchmakingStatusResponse>(jsonString);
                }
            }
            catch(JsonReaderException)
            {
                // Package Type was valid and could be extracted from json string but the rest of the content didn't match and caused a JSONReader Exception
                return null;
            }
            return null;
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

            if (jsonStrings.Count > 0)
                return jsonStrings.ToArray();

            return null;
        }

        private string ConvertNetworkDataToString(byte[] array)
        {
            return System.Text.Encoding.Default.GetString(array);
        }

    }
}
