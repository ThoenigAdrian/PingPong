using GameLogicLibrary;

namespace NetworkLibrary.Packages
{
    public class ClientControlPackage : ClientRegisteredPackage
    {
        public override PackageType PackageType
        {
            get { return PackageType.ClientControl; }
        }

        public ClientControls ControlInput;
    }
}
