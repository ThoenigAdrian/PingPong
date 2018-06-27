namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    public class ClientInitializeGamePackage : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientInitalizeGamePackage; } }

        public enum RequestType
        {
            StartNew,
            Join,
            Matchmaking
        }

        public RequestType Request { get; set; }
        public int GamePlayerCount { get; set; }
        public int[] PlayerTeamwish;
    }
}
