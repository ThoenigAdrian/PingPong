namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerPlayerIDResponse : PackageInterface
    {
        public override PackageType PackageType { get { return PackageType.ServerPlayerIDResponse; } }

        public int[] m_playerIDs;
    }
}
