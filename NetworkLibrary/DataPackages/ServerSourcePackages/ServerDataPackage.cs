namespace NetworkLibrary.DataPackages
{
    public class ServerDataPackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerData; }
        }

        public float Player1PosX { get; set; }
        public float Player1PosY { get; set; }
        public float Player1DirX { get; set; }
        public float Player1DirY { get; set; }

        public float Player2PosX { get; set; }
        public float Player2PosY { get; set; }
        public float Player2DirX { get; set; }
        public float Player2DirY { get; set; }

        public float BallPosX { get; set; }
        public float BallPosY { get; set; }
        public float BallDirX { get; set; }
        public float BallDirY { get; set; }
    }
}
