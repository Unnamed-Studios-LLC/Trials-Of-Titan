using System;
namespace Utils.NET.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsPrefixAttribute : Attribute
    {
        public string prefix;

        public SettingsPrefixAttribute(string prefix)
        {
            this.prefix = prefix;

            if (prefix.EndsWith("."))
            {
                throw new ArgumentException("Invalid argument. Prefix cannot end with a '.' character.");
            }

            if (prefix.StartsWith("."))
            {
                throw new ArgumentException("Invalid argument. Prefix cannot start with a '.' character.");
            }
        }
    }
}
