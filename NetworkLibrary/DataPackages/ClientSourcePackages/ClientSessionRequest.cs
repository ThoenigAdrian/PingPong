namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    public class ClientSessionRequest : PackageInterface
    {
        public override PackageType PackageType { get { return PackageType.ClientSessionRequest; } }

        public ClientSessionRequest()
        {
            Reconnect = false;
        }

        public ClientSessionRequest(int session)
        {
            Reconnect = true;
            ReconnectSessionID = session;
        }

        public bool Reconnect { get; private set; }

        public int ReconnectSessionID { get; private set; }
    }
}
