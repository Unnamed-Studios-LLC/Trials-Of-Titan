using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets.Models;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Map.Objects.Entities;
using World.Worlds;

namespace World.Map.Spawning
{
    public class OverworldCycle
    {
        private const ushort Valdok = 0x1074;
        private const ushort Balun = 0x1075;
        private const ushort Bhognin = 0x1072;
        private const ushort Rictorn = 0x1071;

        public Dictionary<SoulGroup, Dictionary<ushort, int>> titanSpawnCount = new Dictionary<SoulGroup, Dictionary<ushort, int>>()
        {
            { SoulGroup.Mountains, new Dictionary<ushort, int>{
                { Valdok, 5 }
            } },
            { SoulGroup.Tundra, new Dictionary<ushort, int>{
                { Balun, 5 }
            } },
            { SoulGroup.Desert, new Dictionary<ushort, int>{
                { Bhognin, -1 }
            } },
            { SoulGroup.DarkForest, new Dictionary<ushort, int>{
                { Rictorn, -1 }
            } },
        };

        public Dictionary<ushort, int> titanDeathCount;

        public Dictionary<ushort, TemplateString[]> titanDeathCallouts = new Dictionary<ushort, TemplateString[]>()
        {
            /*
            { Valdok, new TemplateString[] {
                new TemplateString("", '#'),
            } },
            { Balun, new TemplateString[] {
                new TemplateString("", '#'),
            } },
            */
        };

        public Dictionary<ushort, TemplateString[]> titanEntranceCallouts = new Dictionary<ushort, TemplateString[]>()
        {
            /*
            { Valdok, new TemplateString[] {
                new TemplateString("", '#'),
            } },
            { Balun, new TemplateString[] {
                new TemplateString("", '#'),
            } },
            */
        };

        private TemplateString[] genericDeathCallouts = new TemplateString[]
        {
            new TemplateString("#enemyShort will return to destroy you #player!", '#'),
            new TemplateString("#player how dare you challenge the might of #enemyShort", '#'),
            new TemplateString("#player your strength is a fluke, #enemyShort will return to defeat you!", '#'),
        };

        private TemplateString[] genericEntranceCallouts = new TemplateString[]
        {
            new TemplateString("#enemyShort will crush you!", '#'),
            new TemplateString("#enemyShort will see that you are dealt with!", '#'),
            new TemplateString("I entrust #enemyShort with your destruction.", '#'),
            new TemplateString("Let's see how you handle the might of #enemyShort", '#'),
        };

        private Overworld overworld;

        private bool spawnedMannah = false;

        public OverworldCycle(Overworld overworld)
        {
            this.overworld = overworld;
            titanDeathCount = new Dictionary<ushort, int>();
            foreach (var dict in titanSpawnCount.Values)
                foreach (var pair in dict)
                    titanDeathCount.Add(pair.Key, pair.Value);

            overworld.spawnSystem.onEncounterKilled += OnEncounterDeath;
        }

        public int GetTitanCount(SoulGroup soulGroup)
        {
            if (!titanSpawnCount.TryGetValue(soulGroup, out var counts))
                return 0;

            int count = 0;
            foreach (var pair in counts)
                if (pair.Value != 0)
                    count++;
            return count;
        }

        public ushort GetTitanSpawn(SoulGroup soulGroup)
        {
            if (!titanSpawnCount.TryGetValue(soulGroup, out var counts))
                return 0;

            var types = counts.Keys.ToArray();
            while (true)
            {
                var type = types.Random();
                var count = counts[type];
                if (count == 0) continue;
                if (count > 0)
                {
                    count--;
                    counts[type] = count;
                }

                DoEntranceCallout(type);

                return type;
            }
        }

        private void OnEncounterDeath(Enemy enemy, Player player, SoulGroup soulGroup)
        {
            UpdateDeathCount(soulGroup, enemy);

            if (!titanDeathCount.TryGetValue(enemy.info.id, out var count)) return;
            var enemyInfo = (EnemyInfo)enemy.info;
            if (!enemyInfo.titan) return;
            DoDeathCallout(enemy, player);
        }

        private void UpdateDeathCount(SoulGroup soulGroup, Enemy enemy)
        {
            if (spawnedMannah) return;
            if (!titanDeathCount.TryGetValue(enemy.info.id, out var count)) return;
            if (count <= 0) return;
            count--;
            titanDeathCount[enemy.info.id] = count;

            TrySpawnMannah();
        }

        private void TrySpawnMannah()
        {
            if (spawnedMannah) return;
            foreach (var countPair in titanDeathCount)
                if (countPair.Value > 0)
                {
                    //Log.Error($"{GameData.objects[countPair.Key].name} : {countPair.Value}");
                    return;
                }

            spawnedMannah = true;
            overworld.spawnSystem.SpawnMannah();
            Callout("Enough of this pestering. I will deal with you myself!");
        }

        private void DoEntranceCallout(ushort enemyType)
        {
            var info = (EnemyInfo)GameData.objects[enemyType];
            if (!titanEntranceCallouts.TryGetValue(info.id, out var callouts))
                callouts = genericEntranceCallouts;

            if (!info.titan) return;

            var text = callouts.Random();

            var values = new Dictionary<string, string>()
            {
                { "enemyShort", info.shortName },
                { "enemy", info.title },
            };

            Callout(text.Build(values));
        }

        private void DoDeathCallout(Enemy enemy, Player player)
        {
            if (!titanDeathCallouts.TryGetValue(enemy.info.id, out var callouts))
                callouts = genericDeathCallouts;

            var text = callouts.Random();

            var values = new Dictionary<string, string>()
            {
                { "player", player.playerName.Value },
                { "enemyShort", ((EnemyInfo)enemy.info).shortName },
                { "enemy", ((EnemyInfo)enemy.info).title },
            };

            Callout(text.Build(values));
        }

        private void Callout(string text)
        {
            var chat = ChatData.Mannah(text);
            overworld.manager.DispatchWorldAction(worlds =>
            {
                foreach (var world in worlds)
                {
                    foreach (var player in world.objects.players.Values)
                        player.AddChat(chat);
                }
            });
        }
    }
}
