using GameLogicLibrary;

namespace NetworkLibrary.Packages
{
    class ClientAddPlayerRequest : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientAddRequest; } }

        public Teams RequestedTeam { get; set; }
    }
}
