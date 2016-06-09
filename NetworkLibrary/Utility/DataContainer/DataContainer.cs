namespace NetworkLibrary.Utility
{
    public abstract class DataContainer<T>
    {
        public abstract T Read();
        public abstract void Write(T data);
    }
}
