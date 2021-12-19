using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.GameState;
using World.Map.Objects.Entities;
using World.Map.Objects.Interfaces;

namespace World.Map.Objects.Map
{
    public class Gravestone : GameObject, IInteractable
    {
        public override GameObjectType Type => GameObjectType.Gravestone;

        public override bool Ticks => true;

        private static ushort[] flowerTypes = new ushort[] { 0xa2c, 0xa2d };

        private HashSet<ulong> flowers = new HashSet<ulong>();

        private ObjectStat<string> playerName = new ObjectStat<string>(ObjectStatType.Name, ObjectStatScope.Public, "", "");

        private double startTime = 0;

        public float lifetime = 0;

        public Gravestone(string playerName)
        {
            this.playerName.Value = playerName;
        }

        public override void OnAddToWorld()
        {
            base.OnAddToWorld();

            startTime = world.time.totalTime;
        }

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.Add(playerName);
        }

        public void Interact(Player player, TnInteract interact)
        {
            if (flowers.Count >= 10 || !flowers.Add(player.GetOwnerId())) return;
            SpawnFlower();
        }

        private void SpawnFlower()
        {
            var flower = new StaticObject();
            flower.Initialize(GameData.objects[GetFlowerType()]);
            flower.position.Value = position.Value + Vec2.FromAngle(Rand.AngleValue()) * (0.1f + 0.6f * Rand.FloatValue());
            world.objects.AddObject(flower);
        }

        private ushort GetFlowerType()
        {
            return flowerTypes[Rand.Next(flowerTypes.Length)];
        }

        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            if (lifetime == 0) return;

            if (time.totalTime > startTime + lifetime)
            {
                world.objects.RemoveObjectPostLogic(this);
            }
        }
    }
}
