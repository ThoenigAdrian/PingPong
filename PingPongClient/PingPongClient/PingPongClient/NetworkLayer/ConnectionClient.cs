namespace PingPongClient.NetworkLayer
{

    class ConnectionClient : ConnectionInterface
    {
        public ConnectionClient(IPEndPoint server)
            : base(server)
        {
            UDPInAdapter = new 
        }

        public void SendClientControl(ClientControlPackage package)
        {
            SendClientDataTCP(package);
        }

        public ServerDataPackage GetServerData()
        {
            return GetServerDataUDP<ServerDataPackage>();
        }
    }
}