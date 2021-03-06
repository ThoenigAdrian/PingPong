﻿namespace NetworkLibrary.DataPackages.ClientSourcePackages
{
    public class ClientSessionRequest : PackageInterface
    {
        public override PackageType PackageType { get { return PackageType.ClientSessionRequest; } }

        public bool Reconnect { get; set; }

        public int ReconnectSessionID { get; set; }
    }
}
