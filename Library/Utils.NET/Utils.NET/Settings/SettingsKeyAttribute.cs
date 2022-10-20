using System;
namespace Utils.NET.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsKeyAttribute : Attribute
    {
        public readonly string key;

        public SettingsKeyAttribute(string key)
        {
            this.key = key;
        }
    }
}
