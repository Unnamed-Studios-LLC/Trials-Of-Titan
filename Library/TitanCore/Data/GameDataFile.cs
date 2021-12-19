using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Utils.NET.IO.Xml;
using Utils.NET.Logging;
using Utils.NET.Utils;

namespace TitanCore.Data
{
    /// <summary>
    /// Object containing GameObjectInfo data loaded from a file
    /// </summary>
    public class GameDataFile
    {
        /// <summary>
        /// type factory used to create game object infos
        /// </summary>
        private static TypeFactory<GameObjectType, GameObjectInfo> infoFactory = new TypeFactory<GameObjectType, GameObjectInfo>(_ => _.Type);

        /// <summary>
        /// Loads a GameDataFile from a given file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static GameDataFile Load(string filePath)
        {
            return Load(File.OpenRead(filePath));
        }

        /// <summary>
        /// Loads a GameDataFile from a given stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static GameDataFile Load(Stream stream)
        {
            var file = new GameDataFile();
            file.infos = new List<GameObjectInfo>();

            using (var reader = XmlReader.Create(stream))
            {
                bool foundObjects = false;
                while (!foundObjects && reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            foundObjects = reader.Name == "Objects";
                            break;
                    }
                }

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            XElement xml = XElement.ReadFrom(reader) as XElement;
                            var parser = new XmlParser(xml);
                            var obj = infoFactory.Create(parser.Enum("Type", GameObjectType.Character));
                            if (obj != null)
                            {
                                obj.Parse(parser);
                                file.infos.Add(obj);
                            }
                            break;
                    }
                }
            }
            return file;
        }

        /// <summary>
        /// An array of all GameObjectInfos loaded from file
        /// </summary>
        public List<GameObjectInfo> infos;
    }
}
