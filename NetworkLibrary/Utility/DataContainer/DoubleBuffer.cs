namespace NetworkLibrary.Utility
{
    public class DoubleBuffer<T> : DataContainer<T>
    {
        T Buffer1 { get; set; }
        T Buffer2 { get; set; }

        bool Switch { get; set; }

        public DoubleBuffer()
        {
            Initialize();
        }

        private void Initialize()
        {
            Switch = false;
            Buffer1 = default(T);
            Buffer2 = default(T);
        }

        public override T Read()
        {
            if (Switch)
                return Buffer1;
            else
                return Buffer2;
        }

        public override void Write(T buffer)
        {
            if (!Switch)
                Buffer1 = buffer;
            else
                Buffer2 = buffer;

            Switch = !Switch;
        }
    }
}
