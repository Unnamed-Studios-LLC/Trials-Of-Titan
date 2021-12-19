using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Map.Objects.Entities;

namespace World.GameState
{
    public struct EntitySnapshot
    {
        public static EntitySnapshot GetDefault(GameObjectInfo info)
        {
            var snapshot = new EntitySnapshot
            {
                time = 0,
                targetPosition = default,
                stopped = true,
                radius = 0,
                health = 100,
                size = info.size.min
            };

            if (info is EnemyInfo enemyInfo)
            {
                snapshot.defense = enemyInfo.defense;
            }
            return snapshot;
        }

        /// <summary>
        /// The client time that this snapshot was processed
        /// </summary>
        public uint time;

        /// <summary>
        /// The target position processed by the client at the time
        /// </summary>
        public Vec2 targetPosition;

        /// <summary>
        /// If the entity was flagged as stopped
        /// </summary>
        public bool stopped;

        /// <summary>
        /// The radius of the entity
        /// </summary>
        public float radius;

        /// <summary>
        /// The health of the entity
        /// </summary>
        public int health;

        /// <summary>
        /// The defense of the entity
        /// </summary>
        public int defense;

        /// <summary>
        /// The size of this entity
        /// </summary>
        public float size;

        /// <summary>
        /// The server status effects
        /// </summary>
        public uint serverEffects;

        public EntitySnapshot(uint time, NetStat[] stats, EntitySnapshot previousSnapshot)
        {
            this.time = time;
            stopped = previousSnapshot.stopped;
            targetPosition = previousSnapshot.targetPosition;
            radius = 0.4f;
            health = previousSnapshot.health;
            defense = previousSnapshot.defense;
            size = previousSnapshot.size;
            serverEffects = previousSnapshot.serverEffects;

            bool posUpdated = false;
            foreach (var stat in stats)
            {
                switch (stat.type)
                {
                    case ObjectStatType.Position:
                        var newPos = (Vec2)stat.value;
                        stopped = (newPos == targetPosition);
                        targetPosition = newPos;
                        posUpdated = true;
                        break;
                    case ObjectStatType.Health:
                        health = (int)stat.value;
                        break;
                    case ObjectStatType.Defense:
                        defense = (int)stat.value;
                        break;
                    case ObjectStatType.Size:
                        size = (float)stat.value;
                        break;
                    case ObjectStatType.StatusEffects:
                        serverEffects = (uint)stat.value;
                        break;
                        /*
                    case ObjectStatType.Stopped:
                        if ((bool)stat.value)
                            stopped = true;
                        //stopped = (bool)stat.value;
                        break;
                        */
                }
            }

            if (!posUpdated)
                stopped = true;
        }

        public bool HasServerEffect(StatusEffect effect)
        {
            return ((serverEffects >> (int)effect) & 1) == 1;
        }
    }

    public class EntityState
    {
        /// <summary>
        /// The id of this entity within the world
        /// </summary>
        public uint gameId;

        /// <summary>
        /// The current starting position
        /// </summary>
        public Vec2 currentPosition;

        /// <summary>
        /// The last time the position was changed
        /// </summary>
        public uint startMoveTime;

        /// <summary>
        /// The currrent snapshot
        /// </summary>
        public EntitySnapshot currentSnapshot;

        public EntityState(uint gameId, EntitySnapshot initialSnapshot)
        {
            this.gameId = gameId;
            currentPosition = initialSnapshot.targetPosition;
            currentSnapshot = initialSnapshot;
            startMoveTime = initialSnapshot.time;
        }

        public void PushSnapshot(EntitySnapshot snapshot)
        {
            if (!currentSnapshot.stopped || !snapshot.stopped)
            {
                currentPosition = GetPosition(snapshot.time);
                startMoveTime = snapshot.time;
            }
            currentSnapshot = snapshot;
        }

        public Vec2 GetPosition(uint time)
        {
            var delta = time - startMoveTime;// currentSnapshot.time;
            if (currentSnapshot.stopped && delta >= 200)
                return currentSnapshot.targetPosition;
            return currentPosition + (currentSnapshot.targetPosition - currentPosition) * (delta * 0.005f);
        }

        public int GetDamageTaken(int damage)
        {
            return StatFunctions.DamageTaken(currentSnapshot.defense, damage, currentSnapshot.HasServerEffect(StatusEffect.Fortified));
        }
    }
}
