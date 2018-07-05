namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerSessionResponse : PackageInterface
    {
        public override PackageType PackageType { get { return PackageType.ServerSessionResponse; } }

        public int ClientSessionID { get; set; }

        public bool GameReconnect { get; set; }
    }
}
