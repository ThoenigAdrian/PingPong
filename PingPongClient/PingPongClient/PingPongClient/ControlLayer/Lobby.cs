namespace PingPongClient.ControlLayer
{
    class Lobby
    {
        public string ServerIP { get; set; }
        public string Status { get; set; }

        public Lobby()
        {
            ServerIP = "";
            Status = "";
        }
    }
}
