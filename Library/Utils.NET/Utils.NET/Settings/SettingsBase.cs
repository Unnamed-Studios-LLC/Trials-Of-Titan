using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Linq;
using Utils.NET.Utils;

namespace Utils.NET.Settings
{
    public abstract class SettingsBase
    {
        /// <summary>
        /// Json file containing setting keys/values
        /// </summary>
        private readonly JObject json;

        /// <summary>
        /// Hashtable mapping property names to keys
        /// </summary>
        private readonly Dictionary<string, string> propertyKeys = new Dictionary<string, string>();

        public SettingsBase(JObject json)
        {
            this.json = json;

            MapKeys();
        }

        /// <summary>
        /// Map property names to json setting keys
        /// </summary>
        private void MapKeys()
        {
            var prefix = GetPrefix();

            foreach (var property in GetType().GetProperties())
            {
                string key;
                if (property.GetCustomAttributes(typeof(SettingsKeyAttribute), true).FirstOrDefault() is SettingsKeyAttribute attribute)
                {
                    key = attribute.key;
                }
                else
                {
                    key = property.Name;
                }
                propertyKeys.Add(property.Name, AddPrefixToKey(key, prefix));
            }
        }

        /// <summary>
        /// Adds a prefix to a given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private string AddPrefixToKey(string key, string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return key;
            return $"{prefix}.{key}";
        }

        /// <summary>
        /// Returns the prefix of this class, if exists
        /// </summary>
        /// <returns></returns>
        private string GetPrefix()
        {
            var attributes = GetType().GetCustomAttributes(typeof(SettingsPrefixAttribute), true);
            if (attributes == null) return string.Empty;
            var builder = new StringBuilder();

            foreach (var attribute in attributes)
            {
                if (builder.Length != 0)
                {
                    builder.Append('.');
                }
                builder.Append(((SettingsPrefixAttribute)attribute).prefix);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the value for a given setting property. Returns default value if no value exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public T GetValue<T>(T defaultValue = default, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                return defaultValue;
            }

            // get json key
            if (!propertyKeys.TryGetValue(propertyName, out var key))
            {
                return defaultValue;
            }

            // no json assigned
            if (json == null) return defaultValue;

            var value = json.GetValue(key);
            if (value == null) return defaultValue;
            return value.Value<T>();
        }
    }
}
