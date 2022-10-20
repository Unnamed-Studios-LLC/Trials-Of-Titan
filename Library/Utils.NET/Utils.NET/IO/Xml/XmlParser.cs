using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Utils.NET.Utils;

namespace Utils.NET.IO.Xml
{
    public class XmlParser
    {
        /// <summary>
        /// The xml node to parse
        /// </summary>
        private readonly XElement xml;

        /// <summary>
        /// The string value of this element
        /// </summary>
        public string stringValue => xml.Value;

        /// <summary>
        /// The integer value of this element
        /// </summary>
        public int intValue => Convert.ToInt32(xml.Value, CultureInfo.InvariantCulture);

        /// <summary>
        /// The integer value of this element, converted from a hexadecimal string
        /// </summary>
        public uint hexValue => StringUtils.ParseHex(xml.Value);


        public XmlParser(XElement xml)
        {
            this.xml = xml;
        }

        /// <summary>
        /// Returns all children elements with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<XmlParser> Elements(string name)
        {
            foreach (var element in xml.Elements(name))
                yield return new XmlParser(element);
        }

        /// <summary>
        /// Attempts to return the element with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string name, out XElement value)
        {
            value = xml.Element(name);
            return value != null;
        }

        /// <summary>
        /// Returns the string value of the node
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string String(string name, string defaultValue = "")
        {
            if (!TryGetValue(name, out var value)) return defaultValue;
            return value.Value;
        }

        /// <summary>
        /// Returns an integer representation of the node's value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int Int(string name, int defaultValue = 0)
        {
            if (!TryGetValue(name, out var value)) return defaultValue;
            return Convert.ToInt32(value.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns an float representation of the node's value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public float Float(string name, float defaultValue = 0)
        {
            if (!TryGetValue(name, out var value)) return defaultValue;
            return Convert.ToSingle(value.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the integer value of a hexadecimal string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public uint Hex(string name, uint defaultValue = 0)
        {
            if (!TryGetValue(name, out var value)) return defaultValue;
            return StringUtils.ParseHex(value.Value);
        }

        /// <summary>
        /// Returns the given enum representation of the element value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T Enum<T>(string name, T defaultValue) where T : struct
        {
            if (!TryGetValue(name, out var value)) return defaultValue;
            if (!System.Enum.TryParse(value.Value, true, out T result)) return defaultValue;
            return result;
        }

        /// <summary>
        /// Returns true if the node exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Exists(string name)
        {
            return xml.Element(name) != null;
        }

        /// <summary>
        /// Attempts to return the element with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetAttribute(string name, out XAttribute value)
        {
            value = xml.Attribute(name);
            return value != null;
        }

        /// <summary>
        /// Returns the string value of the attribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string AtrString(string name, string defaultValue = "")
        {
            if (!TryGetAttribute(name, out var value)) return defaultValue;
            return value.Value;
        }

        /// <summary>
        /// Returns an integer representation of the attribute's value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int AtrInt(string name, int defaultValue = 0)
        {
            if (!TryGetAttribute(name, out var value)) return defaultValue;
            return Convert.ToInt32(value.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns an float representation of the attribute's value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public float AtrFloat(string name, float defaultValue = 0)
        {
            if (!TryGetAttribute(name, out var value)) return defaultValue;
            return Convert.ToSingle(value.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the integer value of a hexadecimal attribute value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public uint AtrHex(string name, uint defaultValue = 0)
        {
            if (!TryGetAttribute(name, out var value)) return defaultValue;
            return StringUtils.ParseHex(value.Value);
        }

        /// <summary>
        /// Returns the given enum representation of the element value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T AtrEnum<T>(string name, T defaultValue) where T : struct
        {
            if (!TryGetAttribute(name, out var value)) return defaultValue;
            if (!System.Enum.TryParse(value.Value, true, out T result)) return defaultValue;
            return result;
        }

        /// <summary>
        /// Returns true if the attribute exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AtrExists(string name)
        {
            return xml.Attribute(name) != null;
        }
    }
}
