namespace Utils.NET.IO.Serialization
{
    internal delegate TField GetDelegate<TField>(object obj);

    internal delegate void SetDelegate<TField>(object obj, TField value);

    internal delegate object CreateDelegate();
}
