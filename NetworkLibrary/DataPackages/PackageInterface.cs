namespace NetworkLibrary.DataPackages
{
    public abstract class PackageInterface
    {
        public abstract PackageType PackageType { get; }
    }

    public enum PackageType
    {
        ClientControl,
        ClientPlayerMovement,
        OpenPort,
        ClientInitalizeGamePackage,
        ClientJoinGameRequest,
        ClientSessionRequest,
        ClientRejoinGamePackage,

        ServerData,
        ServerGameControl,
        ServerAddPlayerResponsePackage,
        ServerJoinGameResponsePackage,
        ServerSessionResponse,
        ServerPlayerIDResponse,
        ServerGameValidResponse,
        ServerGameInvalidResponse
    }

    public enum Result
    {
        Success,
        Fail
    }
}
