namespace Utils.NET.IO.Serialization.Accessors
{
    internal abstract class Processor<TField>
    {
        public abstract TField ReadElement(BitReader r);

        public abstract void WriteElement(BitWriter w, TField element);
    }
}
