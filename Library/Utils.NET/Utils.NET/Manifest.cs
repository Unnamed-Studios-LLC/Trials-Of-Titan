using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utils.NET
{
    /// <summary>
    /// quickly reconstructed class, these values should be loaded from file
    /// </summary>
    public class Manifest
    {
        private readonly JObject _json;

        public Manifest()
        {
            _json = JObject.Parse(File.ReadAllText("manifest.mfst"));
        }

        public bool local => true;

        public string Value(string name, string defaultValue) => _json.GetValue(name)?.Value<string>() ?? defaultValue;
        public T Value<T>(string name, T defaultValue)
        {
            var token = _json.GetValue(name);
            if (token == null) return defaultValue;
            return token.Value<T>();
        }
    }
}
