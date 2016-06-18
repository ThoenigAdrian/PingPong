using NetworkLibrary.DataPackages;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.NetworkImplementations.Network;
using System.Collections.Generic;

namespace NetworkLibrary.NetworkImplementations
{
    public abstract partial class NetworkInterface
    {
        protected class NetworkInput
        {
            NetworkConnectionPool ClientConnections { get; set; }

            public NetworkInput(NetworkConnectionPool connections)
            {
                ClientConnections = connections;
            }

            public PackageInterface GetDataTCP(int session)
            {
                NetworkConnection sessionConnection = ClientConnections[session];

                if (sessionConnection != null)
                    return sessionConnection.ReadTCP();

                return null;
            }

            public PackageInterface[] GetAllDataTCP(int session)
            {
                List<PackageInterface> packages = new List<PackageInterface>();

                PackageInterface package;
                while ((package = GetDataTCP(session)) != null)
                {
                    packages.Add(package);
                }

                return packages.ToArray();
            }

            public Dictionary<int, PackageInterface[]> GetDataFromEverySessionTCP()
            {
                Dictionary<int, PackageInterface[]> packages = new Dictionary<int, PackageInterface[]>();

                foreach (int session in ClientConnections.Keys)
                {
                    PackageInterface[] sessionPackages = GetAllDataTCP(session);
                    if (sessionPackages != null)
                        packages.Add(session, sessionPackages);
                }

                return packages;
            }

            public PackageInterface GetDataUDP(int session)
            {
                NetworkConnection sessionConnection = ClientConnections[session];

                if (sessionConnection != null)
                    return sessionConnection.ReadUDP();

                return null;
            }

            public Dictionary<int, PackageInterface> GetDataFromEverySessionUDP()
            {
                Dictionary<int, PackageInterface> packages = new Dictionary<int, PackageInterface>();

                foreach (int session in ClientConnections.Keys)
                {
                    PackageInterface sessionPackage = GetDataUDP(session);
                    if (sessionPackage != null)
                        packages.Add(session, sessionPackage);
                }

                return packages;
            }
        }
    }
}
