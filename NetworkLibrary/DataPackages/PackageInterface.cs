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
        ClientAddRequest,
        ClientInitalizeGamePackage,
        ClientJoinGameRequest,

        ServerData,
        ServerGameControl,
        ServerAddPlayerResponsePackage,
        ServerJoinGameResponsePackage
        
    }

    public enum Result
    {
        Success,
        Fail
    }
}
