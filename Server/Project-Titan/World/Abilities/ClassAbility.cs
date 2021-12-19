using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.GameState;
using World.Map.Objects.Entities;

namespace World.Abilities
{
    public abstract class ClassAbility
    {
        private static TypeFactory<ClassType, ClassAbility> classAbilityFactory = new TypeFactory<ClassType, ClassAbility>(_ => _.ClassType);

        public static ClassAbility GetAbility(ClassType classType)
        {
            return classAbilityFactory.Create(classType);
        }

        public abstract ClassType ClassType { get; }

        protected Player player;

        protected PlayerState PlayerState => player.gameState.playerState;

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        public abstract void OnHit(EntityState entity, uint time, ref int damageTaken);

        public abstract TnPlayEffect UseAbility(uint time, Vec2 position, Vec2 target, byte value, int attack, ref byte rage, out byte rageCost, out bool sendToSelf, out bool failedToUse);

        public abstract void OnMove(Vec2 position, uint time);
    }
}
