namespace Utils.NET.IO.Serialization.Accessors
{
    internal interface IAccessor
    {
        void Read(BitReader r, object obj);

        void Write(BitWriter w, object obj);
    }
}
