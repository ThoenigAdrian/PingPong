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
        OpenPort,
        ClientInitalizeGamePackage,
        ClientSessionRequest,

        ServerData,
        ServerGameControl,
        ServerSessionResponse,
        ServerPlayerIDResponse,
        ServerMatchmakingStatusResponse
    }

    public enum Result
    {
        Success,
        Fail
    }
}
