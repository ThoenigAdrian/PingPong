using GameLogicLibrary;

namespace NetworkLibrary.DataStructs
{
    public class ClientControlPackage : PackageInterface
    {
        public override PackageType PackageType
        {
            get { return PackageType.ClientControl; }
        }

        public ClientControls ControlInput;
    }
}
