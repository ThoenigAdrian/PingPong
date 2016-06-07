namespace NetworkLibrary.DataStructs
{
    public class ServerDataPackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerData; }
        }

        public int TestValue { get; set; }
    }
}
