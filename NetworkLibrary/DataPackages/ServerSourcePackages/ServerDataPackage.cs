using System.Collections.Generic;

namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerDataPackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerData; }
        }

        public List<Player> PlayerList = new List<Player>();
        public PingPongBall Ball = new PingPongBall();

        public class Player
        {
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float DirectionX { get; set; }
            public float DirectionY { get; set; }

        }
              

        public class PingPongBall
        {
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float DirectionX { get; set; }
            public float DirectionY { get; set; }
        }
        
    }
}
