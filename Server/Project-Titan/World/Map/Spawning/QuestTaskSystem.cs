using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Map.Objects.Entities;

namespace World.Map.Spawning
{
    public abstract class QuestTask
    {
        public abstract void Begin(World world);

        public abstract bool IsComplete(World world);

        public abstract Enemy GetQuest(Player player);

        public abstract void End(World world);

        public abstract void Tick(World world, ref WorldTime time);
    }

    public class BossTask : QuestTask
    {
        private ushort bossType;

        private Vec2 bossPosition;

        private Enemy boss;

        public BossTask(ushort bossType, Vec2 bossPosition)
        {
            this.bossType = bossType;
            this.bossPosition = bossPosition;
        }

        public BossTask(Enemy boss)
        {
            this.boss = boss;
            this.bossType = boss.info.id;
            this.bossPosition = boss.position.Value;
        }

        public override void Begin(World world)
        {
            if (boss != null) return;

            boss = world.objects.CreateEnemy(bossType);
            boss.position.Value = bossPosition;
            world.objects.AddObject(boss);
        }

        public override void End(World world)
        {
            if (boss.world != null)
                boss.RemoveFromWorld();
        }

        public override bool IsComplete(World world)
        {
            return boss.world == null;
        }

        public override void Tick(World world, ref WorldTime time)
        {
            boss.Tick(ref time);
        }

        public override Enemy GetQuest(Player player)
        {
            return boss;
        }
    }

    public class MultiEnemyTask : QuestTask
    {
        public struct MultiEnemyTaskDesc
        {
            public ushort type;

            public Vec2 position;

            public MultiEnemyTaskDesc(ushort type, Vec2 position)
            {
                this.type = type;
                this.position = position;
            }
        }

        private MultiEnemyTaskDesc[] descs;

        private List<Enemy> enemies = new List<Enemy>();

        public MultiEnemyTask(params MultiEnemyTaskDesc[] descs)
        {
            this.descs = descs;
        }

        public MultiEnemyTask(Enemy[] enemies)
        {
            this.enemies.AddRange(enemies);
        }

        public override void Begin(World world)
        {
            if (enemies.Count > 0) return;

            foreach (var desc in descs)
            {
                var enemy = world.objects.CreateEnemy(desc.type);
                enemy.position.Value = desc.position;
                world.objects.AddObject(enemy);
                enemies.Add(enemy);
            }
        }

        public override void End(World world)
        {
            foreach (var enemy in enemies)
                if (enemy.world != null)
                    enemy.RemoveFromWorld();
        }

        public override bool IsComplete(World world)
        {
            foreach (var enemy in enemies)
                if (enemy.world != null)
                    return false;
            return true;
        }

        public override void Tick(World world, ref WorldTime time)
        {
            foreach (var enemy in enemies)
                if (enemy.world != null)
                    enemy.Tick(ref time);
        }

        public override Enemy GetQuest(Player player)
        {
            return enemies.Closest(_ => _.IsDead ? 9999999999 : _.position.Value.DistanceTo(player.position.Value));
        }
    }

    public class QuestTaskSystem
    {
        private int task = -1;

        private World world;

        private QuestTask[] tasks;

        private QuestTask CurrentTask => task >= 0 && task < tasks.Length ? tasks[task] : null;

        public Action onComplete;

        public QuestTaskSystem(World world, params QuestTask[] tasks)
        {
            this.world = world;
            this.tasks = tasks;
        }

        public void Tick(ref WorldTime time)
        {
            TryIncrement();

            CurrentTask?.Tick(world, ref time);
        }

        private void TryIncrement()
        {
            if (task >= tasks.Length) return;
            if (task < 0)
            {
                task = 0;
                tasks[task].Begin(world);
                return;
            }

            if (tasks[task].IsComplete(world))
            {
                tasks[task].End(world);
                task++;

                if (task < tasks.Length)
                {
                    tasks[task].Begin(world);
                }
                else
                {
                    onComplete?.Invoke();
                }
            }
        }

        public Enemy GetQuest(Player player)
        {
            var cur = CurrentTask;
            if (cur == null) return null;
            return cur.GetQuest(player);
        }
    }
}
