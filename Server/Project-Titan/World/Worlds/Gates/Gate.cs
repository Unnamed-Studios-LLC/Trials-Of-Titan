using System;
using TitanCore.Gen;
using Utils.NET.Geometry;
using World.Map;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;
using World.Map.Spawning;

namespace World.Worlds.Gates
{
    public class Gate : World
    {
        public override bool LimitSight => true;

        protected virtual int TargetPlayers => 2;

        protected virtual float ScalePerPlayer => 0.2f;

        protected virtual int PortalTime => -1;

        private QuestTaskSystem questTaskSystem;

        public Portal portal;

        //public TitanSpawnSystem titanSpawnSystem;

        public bool major;

        public int levelRecommendation;

        private bool finished;

        private DateTime closeTime;

        private DateTime portalRemoveTime;

        protected virtual QuestTaskSystem CreateTasks()
        {
            return null;
        }

        protected void WriteSetPieceGround(TitanCore.Gen.Map map, SetPiece setPiece, Int2 position, bool centered)
        {
            if (centered)
                position -= new Int2(setPiece.file.width / 2, setPiece.file.height / 2);

            for (int y = 0; y < setPiece.file.height; y++)
                for (int x = 0; x < setPiece.file.width; x++)
                {
                    var mapPos = position + new Int2(x, y);
                    var tile = setPiece.file.tiles[x, y];
                    if (tile.tileType > 0)
                    {
                        map.Set(mapPos, map.Get(mapPos) | MapElementType.Ground);
                    }
                }
        }

        protected override void DoInitWorld()
        {
            base.DoInitWorld();

            questTaskSystem = CreateTasks();
            if (questTaskSystem != null)
            {
                questTaskSystem.onComplete = OnGateComplete;
            }

            portalRemoveTime = DateTime.Now.AddSeconds(PortalTime);
        }

        private int lastCount = 0;

        public override void Tick()
        {
            base.Tick();

            if (portal != null)
            {
                int count = objects.players.Count;
                if (count != lastCount)
                {
                    lastCount = count;
                    portal.world.PushTickAction(() =>
                    {
                        var p = portal;
                        if (p == null) return;
                        p.worldName.Value = $"{WorldName} ({count}/{MaxPlayerCount})";
                    });
                }
            }

            var gatePortal = portal;
            if (PortalTime != -1 && gatePortal != null && DateTime.Now > portalRemoveTime)
            {
                gatePortal.world.PushTickAction(() =>
                {
                    gatePortal.RemoveFromWorld();
                });
                portal = null;
            }

            if (questTaskSystem != null)
                questTaskSystem.Tick(ref time);

            if (objects.players.Count == 0 && (finished || portal == null))
            {
                manager.RemoveWorld(this);
            }
        }

        public void ScaleEnemyHp(Enemy enemy)
        {
            var playerCount = enemy.playersSentTo.Count;
            int countOffset = playerCount - TargetPlayers;
            enemy.ScaleHealth((int)(enemy.baseMaxHealth + enemy.baseMaxHealth * (ScalePerPlayer * countOffset)));
        }

        public override void AssignQuest(Player player)
        {
            if (questTaskSystem != null)
                player.SetQuest(questTaskSystem.GetQuest(player));
            else
                base.AssignQuest(player);
        }

        protected virtual void OnGateComplete()
        {
            var gatePortal = portal;
            portal?.world.PushTickAction(() =>
            {
                gatePortal.RemoveFromWorld();
            });
            portal = null;

            finished = true;

            //titanSpawnSystem?.GateCompleted(this);
        }
    }
}
