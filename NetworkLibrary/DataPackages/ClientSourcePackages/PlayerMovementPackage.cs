using GameLogicLibrary;

namespace NetworkLibrary.Packages
{
    public class PlayerMovementPackage : ClientRegisteredPackage
    {
        public override PackageType PackageType { get { return PackageType.ClientPlayerMovement; } }

        public int PlayerID;

        public ClientMovement PlayerMovement;
    }
}
