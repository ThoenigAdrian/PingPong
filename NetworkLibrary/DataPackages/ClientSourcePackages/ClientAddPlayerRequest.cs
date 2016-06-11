using GameLogicLibrary;

namespace NetworkLibrary.DataPackages
{
    public class ClientAddPlayerRequest : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientAddPlayerRequest; } }

        public Teams RequestedTeam { get; set; }
    }
}
