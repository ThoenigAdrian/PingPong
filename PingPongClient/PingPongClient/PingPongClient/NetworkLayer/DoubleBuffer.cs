using NetworkLibrary.DataStructs;

namespace PingPongClient.NetworkLayer
{
    class DoubleBuffer<T>
    {
        T Buffer1 { get; set; }
        T Buffer2 { get; set; }

        bool Switch { get; set; }

        public DoubleBuffer()
        {
            Switch = false;

            Buffer1 = new T;
            Buffer2 = new T();
        }

        public T Read()
        {
            if (Switch)
                return Buffer1;
            else
                return Buffer2;
        }

        public void Write(T buffer)
        {
            if (!Switch)
                Buffer1 = buffer;
            else
                Buffer2 = buffer;

            Switch = !Switch;
        }
    }
}
