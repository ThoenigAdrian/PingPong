using GameLogicLibrary;

namespace NetworkLibrary.DataPackages
{
    public class PlayerMovementPackage : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientPlayerMovement; } }

        public int PlayerID;

        public ClientMovement PlayerMovement;
    }
}
