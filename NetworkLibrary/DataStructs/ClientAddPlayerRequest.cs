using GameLogicLibrary;

namespace NetworkLibrary.DataStructs
{
    class ClientAddPlayerRequest : PackageInterface
    {
        public override PackageType PackageType { get { return PackageType.ClientAddRequest; } }

        public Teams RequestedTeam { get; set; }
    }
}
