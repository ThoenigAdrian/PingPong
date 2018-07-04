namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerAddPlayerResponsePackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerData; }
        }

        public Result Result;
    }
}
