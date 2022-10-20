using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Logging;

namespace Utils.NET.Utils
{
    /// <summary>
    /// A class used to create template strings with easy to replace keys.
    /// <para>Example:</para>
    /// <para>var template = new TemplateString("This is a template string, my name is #name", '#')</para>
    /// </summary>
    public class TemplateString
    {
        private readonly string[] bodyStrings;

        private readonly string[] keys;

        public TemplateString(string template, char keyCharacter)
        {
            bodyStrings = template.Split(keyCharacter);
            keys = new string[bodyStrings.Length - 1];
            for (int i = 1; i < bodyStrings.Length; i++)
            {
                bodyStrings[i] = GetKeyAndTruncate(bodyStrings[i], out var key);
                keys[i - 1] = key;
            }
        }

        /// <summary>
        /// Returns if this template contains the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            for (int i = 0; i < keys.Length; i++)
                if (keys[i].Equals(key, StringComparison.Ordinal))
                    return true;
            return false;
        }

        private string GetKeyAndTruncate(string input, out string word)
        {
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (!char.IsLetter(c))
                {
                    word = input.Substring(0, i);
                    return input.Substring(i);
                }
            }

            word = input;
            return "";
        }

        /// <summary>
        /// Builds the template string with a given dictionary containing key/values
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public string Build(Dictionary<string, string> keyValues)
        {
            var builder = new StringBuilder(bodyStrings[0]);
            for (int i = 1; i < bodyStrings.Length; i++)
            {
                var key = keys[i - 1];
                if (!keyValues.TryGetValue(key, out var value))
                    value = key;

                builder.Append(value);
                builder.Append(bodyStrings[i]);
            }
            return builder.ToString();
        }
    }
}
