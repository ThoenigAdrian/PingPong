using GameLogicLibrary;

namespace NetworkLibrary.DataStructs
{
    public class PlayerMovementPackage : PackageInterface
    {
        public override PackageType PackageType { get { return PackageType.ClientPlayerMovement; } }

        public int PlayerID;

        public ClientMovement PlayerMovement;
    }
}
