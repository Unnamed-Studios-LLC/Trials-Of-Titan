namespace Utils.NET.IO.Serialization
{
    internal interface IBitObjectFactory
    {
        object Read(BitReader r);

        void Write(BitWriter w, object obj);
    }
}
