﻿namespace NetworkLibrary.DataPackages.ServerSourcePackages.Matchmaking
{
    public class ServerMatchmakingStatusResponse : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ServerMatchmakingStatusResponse; }
        }

        public bool GameFound;
        public string Status;
        public bool Error;
    }
}
