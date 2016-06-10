using System.Collections.Generic;

namespace NetworkLibrary.DataPackages
{
    public class ServerDataPackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerData; }
        }

        public class Player
        {
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float DirectionX { get; set; }
            public float DirectionY { get; set; }

        }

        public List<Player> PlayerList = new List<Player>();        

        public float BallPosX { get; set; }
        public float BallPosY { get; set; }
        public float BallDirX { get; set; }
        public float BallDirY { get; set; }
    }
}
