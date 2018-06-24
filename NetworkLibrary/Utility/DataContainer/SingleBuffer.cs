namespace NetworkLibrary.Utility
{
    public class SingleBuffer<T> : DataContainer<T>
    {
        T buffer;

        public override T Read()
        {
            return buffer;
        }

        public override void Write(T data)
        {
            buffer = data;
        }
    }
}
