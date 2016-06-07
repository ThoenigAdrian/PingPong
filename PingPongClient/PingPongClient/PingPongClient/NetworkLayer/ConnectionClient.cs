namespace PingPongClient.NetworkLayer
{

    class ConnectionClient : ConnectionInterface
    {
        public ConnectionClient(IPEndPoint server)
            : base(server)
        {
            UDPOutAdapter = new ClientUDPAdapter();
            UDPInAdapter = new ServerUDPAdapter();

            TCPOutAdapter = new ClientTCPAdapter();
            TCPInAdapter = new ServerTCPAdapter();
        }

        public void SendClientControl(ClientControlPackage package)
        {
            SendClientDataTCP(package);
        }

        public ServerDataPackage GetServerData()
        {
            return GetServerDataUDP() as ServerDataPackage;
        }
    }
}