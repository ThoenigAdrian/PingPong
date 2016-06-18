using NetworkLibrary.DataPackages;
using NetworkLibrary.Utility;

namespace PingPongClient.NetworkLayer
{
    public class SubControlResponseRequest : ResponseRequest
    {
        public enum GameMode
        {
            blabla
        }

        public GameMode Issuer { get; private set; }

        public SubControlResponseRequest(GameMode issuer, PackageType responseType, long timeOutMilliseconds) : base(responseType, timeOutMilliseconds)
        {
            Issuer = issuer;
        }
    }
}
