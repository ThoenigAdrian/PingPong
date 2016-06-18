namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    public class ClientJoinGameRequest : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientJoinGameRequest; } }
        public int GamePlayerCount { get; set; }
        public int[] PlayerTeamwish;
    }
}
