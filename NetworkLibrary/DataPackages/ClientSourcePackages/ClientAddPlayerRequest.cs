using GameLogicLibrary;

namespace NetworkLibrary.DataPackages
{
    class ClientAddPlayerRequest : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientAddRequest; } }

        public Teams RequestedTeam { get; set; }
    }
}
