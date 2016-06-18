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

        ServerData,
        ServerGameControl,
        ServerAddPlayerResponsePackage,
        ServerJoinGameResponsePackage,
        ServerSessionResponse,
        ServerPlayerIDResponse
    }

    public enum Result
    {
        Success,
        Fail
    }
}
