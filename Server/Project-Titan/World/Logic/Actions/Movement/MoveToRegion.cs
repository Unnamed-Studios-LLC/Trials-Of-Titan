using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public enum ArraySelection
    {
        Random,
        Closest
    }

    public class MoveToRegionValue
    {
        public Vec2 target;

        public float speed;
    }

    public class MoveToRegion : LogicAction<MoveToRegionValue>
    {
        private Range speed;

        private HashSet<Region> regions = new HashSet<Region>();

        private ArraySelection selection = ArraySelection.Closest;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "speed":
                    speed = reader.ReadFloat();
                    return true;
                case "speedMin":
                    speed.min = reader.ReadFloat();
                    return true;
                case "speedMax":
                    speed.max = reader.ReadFloat();
                    return true;
                case "region":
                    var regionString = reader.ReadString();
                    if (!Enum.TryParse(regionString, true, out Region region))
                    {
                        Log.Write("Unable to find region named: " + regionString);
                    }
                    else
                    {
                        regions.Add(region);
                    }
                    return true;
                case "selection":
                    var selectionString = reader.ReadString();
                    if (!Enum.TryParse(selectionString, true, out selection))
                    {
                        Log.Write("Unable to parse selection method: " + selectionString);
                    }
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out MoveToRegionValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new MoveToRegionValue();
            obj.speed = speed.GetRandom();

            var positions = new List<Int2>();
            foreach (var region in regions)
            {
                var points = entity.world.GetRegions(region);
                if (points == null || points.Count == 0) continue;
                positions.AddRange(points);
            }

            if (positions.Count == 0) return;

            Int2 position = Int2.zero;
            switch (selection)
            {
                case ArraySelection.Random:
                    position = positions[Rand.Next(positions.Count)];
                    break;
                case ArraySelection.Closest:
                    position = positions.Closest(_ => entity.position.Value.SqrDistanceTo(_.ToVec2()));
                    break;
            }
            obj.target = position.ToVec2() + 0.5f;
        }

        public override void Tick(Entity entity, ref MoveToRegionValue obj, ref StateContext context, ref WorldTime time)
        {
            if (obj.target == Vec2.zero) return;
            if (!(entity is NotPlayable notPlayable)) return;

            var vector = obj.target - notPlayable.position.Value;
            var speedDistance = obj.speed * (float)time.deltaTime;
            var length = vector.Length;
            if (length < speedDistance)
                notPlayable.MoveTo(obj.target);
            else
                notPlayable.MoveBy(vector.ChangeLength(speedDistance, length));
        }
    }
}
