using GameLogicLibrary;

namespace NetworkLibrary.DataStructs
{
    class ClientControlPackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ClientControl; }
        }

        public ClientControls Input;
    }
}
