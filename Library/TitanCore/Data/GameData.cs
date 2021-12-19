using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using Utils.NET.Logging;
using Utils.NET.Utils;

namespace TitanCore.Data
{
    public static class GameData
    {
        /// <summary>
        /// All game objects
        /// </summary>
        public static Dictionary<ushort, GameObjectInfo> objects = new Dictionary<ushort, GameObjectInfo>();

        /// <summary>
        /// All classes that are playable
        /// </summary>
        public static List<CharacterInfo> playableClasses = new List<CharacterInfo>();

        /// <summary>
        /// All game objects keyed by their lowercase name
        /// </summary>
        private static Dictionary<string, GameObjectInfo> nameToObjects = new Dictionary<string, GameObjectInfo>();

        /// <summary>
        /// Emote unlockers mapped to their emote type
        /// </summary>
        private static Dictionary<EmoteType, EmoteUnlockerInfo> emoteInfos = new Dictionary<EmoteType, EmoteUnlockerInfo>();

        /// <summary>
        /// Loads all game data in a given directory path
        /// </summary>
        /// <param name="directory"></param>
        public static void LoadDirectory(string directory, bool overwrite = true)
        {
            LoadFiles(Directory.EnumerateFiles(directory), _ => File.OpenRead(_), overwrite);
        }

        public static void LoadFiles(IEnumerable<string> files, Func<string, Stream> reader, bool overwrite = true)
        {
            if (!overwrite && objects.Count > 0) return;
            ClearObjects();
            int count = 0;
            foreach (var file in files)
            {
                //if (!file.EndsWith(".xml")) continue; // exclude non-xmls
                var fileName = Path.GetFileName(file);
                Log.Write($"Loading data file {fileName}", ConsoleColor.Yellow);
                AddFile(GameDataFile.Load(reader(file)));
                Log.Write($"Loaded {objects.Count - count} objects from {fileName}", ConsoleColor.Green);
                count = objects.Count;
            }
            Log.Write($"Loaded {objects.Count} objects.", ConsoleColor.Green);
        }

        /// <summary>
        /// Clears all loaded objects
        /// </summary>
        public static void ClearObjects()
        {
            objects.Clear();
            nameToObjects.Clear();
        }

        /// <summary>
        /// Returns an object that was keyed to the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObjectInfo GetObjectByName(string name)
        {
            if (nameToObjects.TryGetValue(name.ToLower(), out var info))
                return info;
            return null;
        }

        /// <summary>
        /// Adds a given file to the game data
        /// </summary>
        /// <param name="file"></param>
        public static void AddFile(GameDataFile file)
        {
            foreach (var info in file.infos)
                AddInfo(info);
        }

        private static void ProcessInfo(GameObjectInfo info)
        {
            switch (info.Type)
            {
                case GameObjectType.Character:
                    var classInfo = (CharacterInfo)info;
                    if (classInfo.notPlayable || classInfo.serverOnly) break;
                    playableClasses.Add(classInfo);
                    break;
                case GameObjectType.Weapon:
                    var weaponInfo = (WeaponInfo)info;
                    foreach (var proj in weaponInfo.projectiles)
                        proj.ProcessReference();
                    break;
                case GameObjectType.EmoteUnlocker:
                    var emoteUnlocker = (EmoteUnlockerInfo)info;
                    emoteInfos.Add(emoteUnlocker.emoteType, emoteUnlocker);
                    break;
            }
        }

        /// <summary>
        /// Adds a GameObjectInfo to the game data
        /// </summary>
        /// <param name="info"></param>
        private static void AddInfo(GameObjectInfo info)
        {
            ProcessInfo(info);
            if (objects.TryGetValue(info.id, out var duplicate)) // check for id duplicate
            {
                Log.Write($"[GameData] Duplicate id ({info.id}) for \"{info.name}\" and \"{duplicate.name}\"");
            }
            else
            {
                objects.Add(info.id, info); // add info to the object dict
            }

            string name = info.name.ToLower();
            if (nameToObjects.TryGetValue(name, out duplicate)) // check for name duplicate
            {
                Log.Write($"[GameData] Duplicate name found: \"{info.name}\"");
            }
            else
            {
                nameToObjects.Add(name, info); // add info to the name dict
            }
        }

        public static IEnumerable<GameObjectInfo> Search(string pattern)
        {
            foreach (var obj in objects.Values)
            {
                if (StringUtils.DoesMatchPattern(obj.name, pattern))
                    yield return obj;
                else if (obj is EquipmentInfo equip && StringUtils.DoesMatchPattern(equip.slotType.ToString() + equip.tier.ToString(), pattern))
                    yield return obj;
            }
        }

        public static EmoteUnlockerInfo GetEmoteInfo(EmoteType emoteType)
        {
            if (!emoteInfos.TryGetValue(emoteType, out var emoteInfo))
                return null;
            return emoteInfo;
        }
    }
}
