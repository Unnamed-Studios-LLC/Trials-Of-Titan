using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Map.Objects;
using World.Map.Objects.Entities;
using World.Net;

namespace World.GameState
{
    public class PlayerGameState
    {
        private struct Acknowledgement
        {
            public uint time;

            public uint tickId;

            public Acknowledgement(uint time, uint tickId)
            {
                this.time = time;
                this.tickId = tickId;
            }
        }

        /// <summary>
        /// The player that this game state belongs to
        /// </summary>
        private Player player;

        /// <summary>
        /// The tiles discovered by the player
        /// </summary>
        private bool[,] discoveredTiles;

        /// <summary>
        /// Objects that this player is receiving updates on
        /// </summary>
        private Dictionary<uint, GameObject> sentObjects = new Dictionary<uint, GameObject>();

        /// <summary>
        /// Object id's that have been processed in this tick
        /// </summary>
        private HashSet<uint> processedObjectIds = new HashSet<uint>();

        /// <summary>
        /// Stack of server damages ready to send to the player
        /// </summary>
        //private Stack<ServerDamage> serverDamages = new Stack<ServerDamage>();

        /// <summary>
        /// Objects that are new to the player
        /// </summary>
        private Stack<NewObjectStats> newObjects = new Stack<NewObjectStats>();

        /// <summary>
        /// Objects that have had their stats updated
        /// </summary>
        private Stack<UpdatedObjectStats> updatedObjects = new Stack<UpdatedObjectStats>();

        /// <summary>
        /// Objects that have been removed from the player's game state
        /// </summary>
        private Stack<uint> removedObjects = new Stack<uint>();

        /// <summary>
        /// Ally projectiles ready to be sent
        /// </summary>
        private List<AllyProjectile> allyProjectiles = new List<AllyProjectile>();

        /// <summary>
        /// Ally aoe projectiles ready to be sent
        /// </summary>
        private List<AllyAoeProjectile> allyAoeProjectiles = new List<AllyAoeProjectile>();

        /// <summary>
        /// Enemy projectiles ready to be sent
        /// </summary>
        private List<EnemyProjectile> enemyProjectiles = new List<EnemyProjectile>();

        /// <summary>
        /// Enemy aoe projectiles ready to be sent
        /// </summary>
        private List<EnemyAoeProjectile> enemyAoeProjectiles = new List<EnemyAoeProjectile>();

        /// <summary>
        /// The id of the current tick
        /// </summary>
        private uint tickId = 1;

        /// <summary>
        /// Queue of tick id packets ready to be acknowledged
        /// </summary>
        private Queue<TnIdPacket> acknowledgeQueue = new Queue<TnIdPacket>();

        /// <summary>
        /// The state of each entity within the client
        /// </summary>
        private Dictionary<uint, EntityState> entityStates = new Dictionary<uint, EntityState>();

        /// <summary>
        /// The state of all enemy projectiles
        /// </summary>
        private Dictionary<uint, ProjectileState> enemyProjectileStates = new Dictionary<uint, ProjectileState>();

        /// <summary>
        /// The state of all enemy projectiles
        /// </summary>
        private Dictionary<uint, AoeProjectileState> enemyAoeProjectileStates = new Dictionary<uint, AoeProjectileState>();

        /// <summary>
        /// The state of all player projectiles
        /// </summary>
        private Dictionary<uint, ProjectileState> playerProjectileStates = new Dictionary<uint, ProjectileState>();

        /// <summary>
        /// The state of all player projectiles
        /// </summary>
        private Dictionary<uint, AoeProjectileState> playerAoeProjectileStates = new Dictionary<uint, AoeProjectileState>();

        /// <summary>
        /// The last tick to block projectile sending
        /// </summary>
        public ulong projectileBlockTick = 0;

        public PlayerState playerState;

        public PlayerGameState(Player player, Int2 worldSize)
        {
            this.player = player;
            discoveredTiles = new bool[worldSize.x, worldSize.y];
        }

        #region Tick

        /// <summary>
        /// Processes an object's stats into the player's game state
        /// </summary>
        /// <param name="gameObject"></param>
        public void ProcessObject(GameObject gameObject, ref WorldTime time)
        {
            if (gameObject == null || processedObjectIds.Contains(gameObject.gameId)) return;

            if (!gameObject.CanShowTo(player)) return;
            if (player.world.LimitSight && gameObject.info.Type != GameObjectType.Character && gameObject != player.quest)
            {
                var objPos = gameObject.position.Value.ToInt2();
                if (!HasDiscoveredTile(objPos.x, objPos.y)) return;
            }

            processedObjectIds.Add(gameObject.gameId);

            gameObject.UpdateStats(time.tickId);

            if (!sentObjects.ContainsKey(gameObject.gameId))
            {
                sentObjects.Add(gameObject.gameId, gameObject);
                gameObject.ProcessedBy(player);
                newObjects.Push(gameObject.GetNewStats(player));
            }
            else
            {
                var updatedStats = gameObject.GetUpdatedStats(player);
                if (updatedStats.stats.Length != 0)
                    updatedObjects.Push(updatedStats);
            }
        }

        /// <summary>
        /// Returns all objects removed from the player's game state since last tick
        /// </summary>
        /// <returns></returns>
        public uint[] GetRemovedObjects()
        {
            var removed = new List<uint>();

            while (removedObjects.Count > 0)
                removed.Add(removedObjects.Pop());

            foreach (var obj in sentObjects.Values.ToArray())
            {
                if (!processedObjectIds.Contains(obj.gameId))
                {
                    obj.RemovedBy(player);
                    sentObjects.Remove(obj.gameId);
                    removed.Add(obj.gameId);
                }
            }
            processedObjectIds.Clear();

            return removed.ToArray();
        }

        public void OnRemovedFromWorld()
        {
            foreach (var obj in sentObjects.Values)
                obj.RemovedBy(player);
        }

        /// <summary>
        /// Gets the next tick packet to send to the player
        /// </summary>
        /// <returns></returns>
        public TnTick GetTick()
        {
            var tick = new TnTick(tickId++, newObjects.ToArray(), updatedObjects.ToArray(), GetRemovedObjects());
            //serverDamages.Clear();
            newObjects.Clear();
            updatedObjects.Clear();
            acknowledgeQueue.Enqueue(tick);

            return tick;
        }

        #endregion

        #region Projectiles

        public void AddEnemyProjectile(Enemy enemy, EnemyProjectile proj)
        {
            if (projectileBlockTick == 0 || player.world.time.tickId - projectileBlockTick < WorldManager.Ticks_Per_Second * 2) return;
            if (!entityStates.TryGetValue(enemy.gameId, out var entityState)) return;
            enemyProjectiles.Add(proj);
            enemyProjectileStates.Add(proj.projectileId, new ProjectileState(enemy, proj));
        }

        public void AddEnemyAoeProjectile(Enemy enemy, EnemyAoeProjectile proj)
        {
            if (projectileBlockTick == 0 || player.world.time.tickId - projectileBlockTick < WorldManager.Ticks_Per_Second * 2) return;
            if (!entityStates.TryGetValue(enemy.gameId, out var entityState)) return;
            enemyAoeProjectiles.Add(proj);
            //enemyProjectileStates.Add(proj.projectileId, new ProjectileState(enemy, proj));
        }

        public void AddEnemyProjectiles(Enemy enemy, EnemyProjectile[] projs)
        {
            if (!entityStates.TryGetValue(enemy.gameId, out var entityState)) return;
            enemyProjectiles.AddRange(projs);
            foreach (var proj in projs)
                enemyProjectileStates.Add(proj.projectileId, new ProjectileState(enemy, proj));
        }

        public void AddEnemyAoeProjectiles(Enemy enemy, EnemyAoeProjectile[] projs)
        {
            if (!entityStates.TryGetValue(enemy.gameId, out var entityState)) return;
            enemyAoeProjectiles.AddRange(projs);
            //foreach (var proj in projs)
            //    enemyProjectileStates.Add(proj.projectileId, new ProjectileState(enemy, proj));
        }

        public void AddAllyProjectile(AllyProjectile proj)
        {
            allyProjectiles.Add(proj);
        }

        public void AddAllyAoeProjectile(AllyAoeProjectile proj)
        {
            allyAoeProjectiles.Add(proj);
        }

        public void AddAllyProjectiles(AllyProjectile[] projs)
        {
            allyProjectiles.AddRange(projs);
        }

        public void AddAllyAoeProjectiles(AllyAoeProjectile[] projs)
        {
            allyAoeProjectiles.AddRange(projs);
        }

        public void AddPlayerProjectile(uint time, Vec2 position, AllyProjectile proj)
        {
            playerProjectileStates.Add(proj.projectileId, new ProjectileState(time, proj, position));
        }

        public void AddPlayerAoeProjectile(uint time, Vec2 position, AllyAoeProjectile proj)
        {
            playerAoeProjectileStates.Add(proj.projectileId, new AoeProjectileState(time, proj));
        }

        public void AddPlayerProjectiles(uint time, Vec2 position, AllyProjectile[] projs)
        {
            foreach (var proj in projs)
                AddPlayerProjectile(time, position, proj);
        }

        public void AddPlayerAoeProjectiles(uint time, Vec2 position, AllyAoeProjectile[] projs)
        {
            foreach (var proj in projs)
                AddPlayerAoeProjectile(time, position, proj);
        }

        /// <summary>
        /// Returns the next projectiles packet
        /// </summary>
        /// <returns></returns>
        public TnProjectiles GetProjectiles()
        {
            if (allyProjectiles.Count == 0 && allyAoeProjectiles.Count == 0 && enemyProjectiles.Count == 0 && enemyAoeProjectiles.Count == 0) return null;
            var projectiles = new TnProjectiles(tickId++, allyProjectiles.ToArray(), allyAoeProjectiles.ToArray(), enemyProjectiles.ToArray(), enemyAoeProjectiles.ToArray());
            allyProjectiles.Clear();
            allyAoeProjectiles.Clear();
            enemyProjectiles.Clear();
            enemyAoeProjectiles.Clear();
            acknowledgeQueue.Enqueue(projectiles);
            return projectiles;
        }

        #endregion

        #region Hit Registration

        public void PlayerHitWall(uint time, uint playerProjId, ushort x, ushort y)
        {
            if (!playerProjectileStates.TryGetValue(playerProjId, out var proj)) // failed to get the player's projectile
            {
                if (!playerAoeProjectileStates.TryGetValue(playerProjId, out var aoeProj))
                {
                    Log.Write("Player projectile does not exist!");
                }
                else
                {
                    PlayerHitWallAoe(time, aoeProj, x, y);
                }

                return;
            }
            
            if (proj.startTime == uint.MaxValue)
            {
                Log.Write("Projectile never acknowledged!");
                return;
            }

            if (time > proj.startTime + (uint)(proj.GetLifetime() * 1000)) // proj exceeded lifetime
            {
                Log.Write("Hit failed! Projectile exceeded lifetime");
                return;
            }

            var projPos = proj.GetPosition(time);

            if ((uint)projPos.x == x && (uint)projPos.y == y) // Hit success!
            {
                player.world.tiles.HitObject(x, y, proj.damage);
                playerProjectileStates.Remove(playerProjId); // TODO add multi-hit / passthrough
            }
            else // Hit failed!
            {
                Log.Write("Hit Wall Failed! Id: " + proj.projectileId);
            }
        }

        private void PlayerHitWallAoe(uint time, AoeProjectileState proj, ushort x, ushort y)
        {
            if (proj.endTime == uint.MaxValue)
            {
                Log.Write("Projectile never acknowledged!");
                return;
            }

            if (proj.endTime != time) // proj exceeded lifetime
            {
                Log.Write("Hit failed! Aoe end time mismatch");
                return;
            }

            var projPos = proj.target;

            if (projPos.DistanceTo(new Vec2(x + 0.5f, y + 0.5f)) < proj.data.radius && proj.wallHitSet.Add(((uint)x << 16) | (uint)y)) // Hit success!
            {
                player.world.tiles.HitObject(x, y, proj.damage);
            }
            else // Hit failed!
            {
                Log.Write("Hit Failed!");
            }
        }

        public void PlayerHitEnemy(uint time, uint playerProjId, uint enemyId)
        {
            if (!playerProjectileStates.TryGetValue(playerProjId, out var proj)) // failed to get the player's projectile
            {
                if (!playerAoeProjectileStates.TryGetValue(playerProjId, out var aoeProj))
                {
                    player.client.SendAsync(new TnError("Hit check failed! Player projectile does not exist!"));
                    //Log.Write("Player projectile does not exist!");
                }
                else
                {
                    PlayerHitEnemyAoe(time, aoeProj, enemyId);
                }

                return;
            }

            if (!entityStates.TryGetValue(enemyId, out var enemyState)) // failed to retrieve the entity that was hit
            {
                //Log.Write("Enemy does not exist to the player!");
                player.client.SendAsync(new TnError("Hit check failed! Enemy does not exist to the player!"));
                return;
            }

            if (proj.startTime == uint.MaxValue)
            {
                //Log.Write("Projectile never acknowledged!");
                player.client.SendAsync(new TnError("Hit check failed! Projectile never acknowledged!"));
                return;
            }

            if (time > proj.startTime + (uint)(proj.GetLifetime() * 1000)) // proj exceeded lifetime
            {
                //Log.Write("Hit failed! Projectile exceeded lifetime");
                player.client.SendAsync(new TnError("Hit check failed! Projectile exceeded lifetime"));
                return;
            }

            var enemyPos = enemyState.GetPosition(time);
            var projPos = proj.GetPosition(time);

            if (projPos.DistanceTo(enemyPos) < proj.radius + enemyState.currentSnapshot.radius * enemyState.currentSnapshot.size) // Hit success!
            {
                bool killed = false;
                int damageTaken = enemyState.GetDamageTaken(proj.damage);
                playerState.ability.OnHit(enemyState, time, ref damageTaken);

                if (player.world.objects.TryGetEnemy(enemyState.gameId, out var enemy))
                {
                    if (((EntityInfo)enemy.info).invincible)
                    {
                        // disconnect
                        player.client.Disconnect();
                        return;
                    }

                    enemyState.currentSnapshot.health -= damageTaken;
                    enemy.Hurt(damageTaken, player);

                    player.OnDamageEnemy(enemy, damageTaken);
                    killed = enemyState.currentSnapshot.health <= 0;
                    if (killed) // kill enemy
                    {
                        entityStates.Remove(enemyState.gameId);
                        enemy.Die(player);
                    }
                }
                else
                {

                }

                playerState.AddRage(time);

                if ((!killed || !proj.data.fallthrough) && !proj.data.ignoreEntity)
                    playerProjectileStates.Remove(playerProjId); // TODO add multi-hit / passthrough
            }
            else // Hit failed!
            {
                Log.Write($"Hit Failed! Id: {proj.projectileId}, time: {time}, stopped: {enemyState.currentSnapshot.stopped}, enemyPos: {enemyPos}, start: {enemyState.currentPosition}, target: {enemyState.currentSnapshot.targetPosition}");
                player.client.SendAsync(new TnError("Hit check failed!"));
                //player.client.Disconnect();
            }
        }

        private void PlayerHitEnemyAoe(uint time, AoeProjectileState proj, uint enemyId)
        {
            if (!entityStates.TryGetValue(enemyId, out var enemyState)) // failed to retrieve the entity that was hit
            {
                player.client.SendAsync(new TnError("Hit check failed! Enemy does not exist to the player!"));
                return;
            }

            if (proj.endTime == uint.MaxValue)
            {
                player.client.SendAsync(new TnError("Hit check failed! Projectile never acknowledged!"));
                return;
            }

            if (proj.endTime != time) // proj exceeded lifetime
            {
                player.client.SendAsync(new TnError("Hit check failed! Hit failed! Aoe end time mismatch"));
                return;
            }

            var enemyPos = enemyState.GetPosition(time);
            var projPos = proj.target;

            if (projPos.DistanceTo(enemyPos) < proj.data.radius && proj.hitSet.Add(enemyId)) // Hit success!
            {
                int damageTaken = enemyState.GetDamageTaken(proj.damage);
                playerState.ability.OnHit(enemyState, time, ref damageTaken);

                if (player.world.objects.TryGetEnemy(enemyState.gameId, out var enemy))
                {
                    enemyState.currentSnapshot.health -= damageTaken;
                    enemy.Hurt(damageTaken, player);
                    player.OnDamageEnemy(enemy, damageTaken);
                    if (enemyState.currentSnapshot.health <= 0) // kill enemy
                    {
                        entityStates.Remove(enemyState.gameId);
                        enemy.Die(player);
                    }
                }
                else
                {

                }

                playerState.AddRage(time);
            }
            else // Hit failed!
            {
                Log.Write($"Hit Failed! Id: {proj.projectileId}, projPos: {projPos}, enemyPos: {enemyPos}");
                player.client.SendAsync(new TnError("Hit check failed! Distance failure."));
            }
        }

        /*
        public void HurtEnemy(EntityState enemyState, ushort damage, bool giveRage)
        {
            if (!player.world.objects.TryGetEnemy(enemyState.gameId, out var enemy)) return;

            int damageTaken = enemyState.GetDamageTaken(damage);
            enemyState.currentSnapshot.health -= damageTaken;
            enemy.Hurt(damageTaken, player);
            player.OnDamageEnemy(enemy, damageTaken);
            if (enemyState.currentSnapshot.health <= 0) // kill enemy
            {
                entityStates.Remove(enemyState.gameId);
                enemy.Die(player);
            }

            playerState.rage += 1;
        }
        */

        public void EnemyHitPlayer(uint time, uint projId, uint clientTickId, Vec2 position)
        {
            if (!enemyProjectileStates.TryGetValue(projId, out var proj)) // failed to get the enemy projectile
            {
                if (!enemyAoeProjectileStates.TryGetValue(projId, out var aoeProj))
                {
                    Log.Write("Enemy projectile does not exist! " + projId);
                    return;
                }

                EnemyHitPlayerAoe(time, aoeProj);
            }

            if (proj.startTime == uint.MaxValue)
            {
                Log.Write("Projectile never acknowledged!");
                return;
            }

            playerState.Damage(time, proj.damage, proj.ownerInfo, proj.ownerId);

            var projPosition = proj.GetPosition(time);
            playerState.AdvancePosition(position, time);
            foreach (var onHit in proj.data.onHitEffects)
            {
                switch (onHit.type)
                {
                    case StatusEffect.Charmed:
                        playerState.AddCharmed(position, projPosition, time, onHit.duration);
                        break;
                    case StatusEffect.KnockedBack:
                        playerState.AddKnockedBack(position, projPosition, time, onHit.duration);
                        break;
                    case StatusEffect.Grounded:
                        playerState.AddGrounded(position, time, onHit.duration);
                        break;
                    case StatusEffect.Mundane:
                        playerState.rage = 0;
                        playerState.AddClientStatusEffect(onHit.type, time, onHit.duration);
                        break;
                    default:
                        playerState.AddClientStatusEffect(onHit.type, time, onHit.duration);
                        break;
                }
            }

            enemyProjectileStates.Remove(projId);
        }

        public void EnemyHitWall(uint time, uint projectileId)
        {
            if (!enemyProjectileStates.TryGetValue(projectileId, out var enemyProj))
            {
                return;
            }

            var pos = enemyProj.GetPosition(time);
            //int x = (int)pos.x, y = (int)pos.y;
            if (player.world.tiles.GetCollisionType(pos.x, pos.y) != Map.CollisionType.None)
            {
                enemyProjectileStates.Remove(projectileId);
            }
        }

        public void EnemyHitPlayerAoe(uint time, AoeProjectileState aoeProj)
        {
            if (aoeProj.endTime == uint.MaxValue)
            {
                Log.Write("Projectile never acknowledged!");
                return;
            }

            playerState.Damage(time, aoeProj.damage, aoeProj.ownerInfo, aoeProj.ownerId);

            enemyAoeProjectileStates.Remove(aoeProj.projectileId);
        }

        private void CheckPlayerHit(Vec2 position, float radius, uint time)
        {
            var removedProjectiles = new List<uint>();

            bool cantBeHit = playerState.HasEffect(StatusEffect.Invincible, time) ||
                playerState.HasEffect(StatusEffect.Invulnerable, time) ||
                playerState.HasEffect(StatusEffect.KnockedBack, time) ||
                playerState.HasEffect(StatusEffect.Grounded, time) ||
                playerState.HasEffect(StatusEffect.Dashing, time);

            foreach (var proj in enemyProjectileStates.Values)
            {
                if (proj.startTime == uint.MaxValue) continue; // projectile not acknowledged yet
                if (time > proj.startTime + (uint)(proj.GetLifetime() * 1000)) // proj exceeded lifetime
                {
                    removedProjectiles.Add(proj.projectileId);
                    continue;
                }

                if (time > proj.hitTime)
                {
                    Log.Error("Kicked for cheating");
                    player.client.Disconnect();
                    return;
                }

                var projPos = proj.GetPosition(time);
                if (!cantBeHit && projPos.DistanceTo(position) * 1.1f < proj.radius + radius) // did hit
                {
                    var dmgTaken = playerState.GetDamageTaken(proj.damage, time);
                    if (dmgTaken == 0) continue;
                    proj.hitTime = time;
                }
            }

            foreach (var removed in removedProjectiles)
            {
                enemyProjectileStates.Remove(removed);
            }

            removedProjectiles.Clear();

            foreach (var proj in playerProjectileStates.Values)
            {
                if (time > proj.startTime + (uint)(proj.GetLifetime() * 1000)) // proj exceeded lifetime
                {
                    removedProjectiles.Add(proj.projectileId);
                }
            }

            foreach (var removed in removedProjectiles)
                playerProjectileStates.Remove(removed);

            removedProjectiles.Clear();

            foreach (var proj in enemyAoeProjectileStates.Values)
            {
                if (time == proj.endTime)
                {
                    if (proj.target.DistanceTo(position) < proj.data.radius) // did hit
                    {
                        proj.didHitPlayer = true;
                    }
                }
                else if (time > proj.endTime)
                {
                    if (proj.didHitPlayer)
                    {
                        //player.client.Disconnect();
                        return;
                    }
                    removedProjectiles.Add(proj.projectileId);
                }
            }

            removedProjectiles.Clear();

            foreach (var removed in removedProjectiles)
                enemyAoeProjectileStates.Remove(removed);

            foreach (var proj in playerAoeProjectileStates.Values)
            {
                if (time > proj.endTime)
                {
                    removedProjectiles.Add(proj.projectileId);
                }
            }

            foreach (var removed in removedProjectiles)
                playerAoeProjectileStates.Remove(removed);

        }

        #endregion

        #region Tiles

        public bool HasDiscoveredTile(int x, int y)
        {
            return discoveredTiles[x, y];
        }
        public void SetDiscoveredTile(int x, int y, bool value)
        {
            discoveredTiles[x, y] = value;
        }

        #endregion

        #region Acknowledgements

        /// <summary>
        /// Acknowledges a packet with the given tickId
        /// </summary>
        /// <param name="tickId"></param>
        public void Acknowledge(uint time, uint tickId)
        {
            if (acknowledgeQueue.Count == 0) // error, no packet to acknowledge. TODO disconnect client
            {
                player.client.SendAsync(new TnError("No packet to acknowledge!"));
                Log.Write("No packet to acknowledge!");
                return;
            }

            var packet = acknowledgeQueue.Dequeue();
            if (packet.tickId != tickId) // tickId mismatch, likely a false tick id being sent. TODO disconnect client
            {
                player.client.SendAsync(new TnError("Acknowledgement mismatch!"));
                Log.Write("Acknowledgement tickId mismatch!");
                return;
            }

            AcknowledgePacket(time, packet);
        }

        private void AcknowledgePacket(uint time, TnIdPacket packet)
        {
            switch (packet)
            {
                case TnTick tick:
                    AcknowledgeTick(time, tick);
                    break;
                case TnProjectiles projectiles:
                    AcknowledgeProjectiles(time, projectiles);
                    break;
            }
        }

        private void AcknowledgeTick(uint time, TnTick tick)
        {
            var processed = new HashSet<uint>();

            foreach (var newObj in tick.newObjects)
            {
                if (newObj.gameId == player.gameId)
                {
                    playerState = new PlayerState(time, player, newObj);
                    continue;
                }

                var info = GameData.objects[newObj.type];
                if (info is CharacterInfo) continue;
                var snapshot = new EntitySnapshot(time, newObj.stats, EntitySnapshot.GetDefault(info));
                var state = new EntityState(newObj.gameId, snapshot);

                entityStates.Add(newObj.gameId, state);
                processed.Add(newObj.gameId);
            }

            foreach (var updObj in tick.updatedObjects)
            {
                if (updObj.gameId == player.gameId)
                {
                    playerState.PushUpd(time, updObj);
                    continue;
                }
                if (!entityStates.TryGetValue(updObj.gameId, out var state)) continue;
                var snapshot = new EntitySnapshot(time, updObj.stats, state.currentSnapshot);
                state.PushSnapshot(snapshot);
                processed.Add(updObj.gameId);
            }

            for (int i = 0; i < tick.removedObjects.Length; i++)
            {
                entityStates.Remove(tick.removedObjects[i]);
            }

            CheckPlayerHit(player.position.Value, playerState.currentSnapshot.radius, time);
        }

        private void AcknowledgeProjectiles(uint time, TnProjectiles projectiles)
        {
            foreach (var enemyProj in projectiles.enemyProjectiles)
            {
                if (!enemyProjectileStates.TryGetValue(enemyProj.projectileId, out var state)) continue;
                state.startTime = time;
            }

            foreach (var enemyAoeProj in projectiles.enemyAoeProjectiles)
            {
                if (!enemyAoeProjectileStates.TryGetValue(enemyAoeProj.projectileId, out var state)) continue;
                state.SetEndTime(time);
            }
        }

        #endregion

        #region Abilities

        public void UseAbility(TnUseAbility useAbility)
        {
            playerState.UseAbility(useAbility.clientTickId * Client.Client_Fixed_Delta, useAbility.position, useAbility.target, useAbility.value);
            //player.RemoveFullSouls((ulong)playerState.UseAbility(useAbility.clientTickId * Client.Client_Fixed_Delta, useAbility.position));
        }

        #endregion
    }
}