namespace NetworkLibrary.DataPackages
{
    public class ClientOpenPortPackage : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.OpenPort; } }
    }
}
