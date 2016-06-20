using GameLogicLibrary.GameObjects;
using System.Collections.Generic;

namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerDataPackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerData; }
        }

        public ServerDataPackage()
        {
            Players = new List<RawPlayer>();
        }

        public List<RawPlayer> Players { get; set; }
        public RawBall Ball { get; set; }

    }
}
