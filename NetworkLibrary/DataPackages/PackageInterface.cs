﻿namespace NetworkLibrary.DataPackages
{
    public abstract class PackageInterface
    {
        public abstract PackageType PackageType { get; }
    }

    public enum PackageType
    {
        ClientControl,
        ClientPlayerMovement,
        ClientAddPlayerRequest,
        ClientInitalizeGamePackage,
        ClientJoinGameRequest,

        ServerData,
        ServerGameControl,
        ServerAddPlayerResponsePackage,
        ServerJoinGameResponsePackage,
        ServerSessionResponse
    }

    public enum Result
    {
        Success,
        Fail
    }
}
