namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    public class ClientInitializeGamePackage : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientInitalizeGamePackage; } }
        public int GamePlayerCount { get; set; }
        public int[] PlayerTeamwish;
    }
}
