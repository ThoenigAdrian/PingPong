using NetworkLibrary.DataStructs;

namespace NetworkLibrary.PackageAdapters
{
    public abstract class PackageAdapterInterface
    {
        public abstract PackageInterface ByteToPackage(byte[] data);
        public abstract byte[] PackageToByte(PackageInterface package);
    }
}
