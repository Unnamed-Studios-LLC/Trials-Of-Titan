using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Logic.Components
{
    public enum PositionSelectionType
    {
        Circle,
        RandomPlayer
    }

    public abstract class PositionSelection
    {
        private static TypeFactory<PositionSelectionType, PositionSelection> positionSelectionFactory = new TypeFactory<PositionSelectionType, PositionSelection>(_ => _.Type);

        public static PositionSelection Create(PositionSelectionType type)
        {
            return positionSelectionFactory.Create(type);
        }

        public abstract PositionSelectionType Type { get; }

        public abstract Vec2 GetRelativeSpawnPosition(Enemy enemy);

        public virtual bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            return false;
        }
    }

    public class PositionCircleSelection : PositionSelection
    {
        public override PositionSelectionType Type => PositionSelectionType.Circle;

        private Range radius = new Range(0, 1);

        private Range arc = new Range(0, AngleUtils.PI_2);

        private Range minionArcOffset = new Range(0, 0);

        private float minionArcExp = 0;

        public PositionCircleSelection(Range radius, Range arc, Range minionArcOffset)
        {
            this.radius = radius;
            this.arc = arc;
            this.minionArcOffset = minionArcOffset;
        }

        public PositionCircleSelection()
        {

        }

        public override Vec2 GetRelativeSpawnPosition(Enemy enemy)
        {
            var arcOffset = minionArcOffset.GetRandom() * enemy.LeaderCount();
            if (minionArcExp != 0)
                arcOffset *= arcOffset * minionArcExp;
            var angle = arc.GetRandom() + arcOffset;
            return Vec2.FromAngle(angle) * radius.GetRandom();
        }

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "radius":
                    radius.min = radius.max = reader.ReadFloat();
                    return true;
                case "radiusMin":
                    radius.min = reader.ReadFloat();
                    return true;
                case "radiusMax":
                    radius.max = reader.ReadFloat();
                    return true;
                case "arc":
                    arc.min = arc.max = reader.ReadAngle();
                    return true;
                case "arcMin":
                    arc.min = reader.ReadAngle();
                    Log.Write(arc.min * AngleUtils.Rad2Deg);
                    return true;
                case "arcMax":
                    arc.max = reader.ReadAngle();
                    Log.Write(arc.max * AngleUtils.Rad2Deg);
                    return true;
                case "minionArcOffset":
                    minionArcOffset.min = minionArcOffset.max = reader.ReadAngle();
                    return true;
                case "minionArcOffsetMin":
                    minionArcOffset.min = reader.ReadAngle();
                    return true;
                case "minionArcOffsetMax":
                    minionArcOffset.max = reader.ReadAngle();
                    return true;
                case "minionArcExp":
                    minionArcExp = reader.ReadFloat();
                    return true;
            }
            return false;
        }
    }

    public class PositionRandomPlayerSelection : PositionSelection
    {
        public override PositionSelectionType Type => PositionSelectionType.RandomPlayer;

        private float searchRadius = 8;

        private Range angleOffset = 0;

        private Range relativeAngleOffset = 0;

        private Range distanceOffset = 0;

        private Range relativeDistanceOffset = 0;

        public PositionRandomPlayerSelection() { }

        public override Vec2 GetRelativeSpawnPosition(Enemy enemy)
        {
            var player = enemy.GetClosestPlayer(searchRadius);
            if (player == null) return Vec2.zero;
            var vector = player.position.Value - enemy.position.Value;
            if (relativeDistanceOffset.max != 0)
            {
                var offset = vector.Angle + relativeAngleOffset.GetRandom();
                vector += Vec2.FromAngle(offset) * relativeDistanceOffset.GetRandom();
            }
            if (distanceOffset.max != 0)
            {
                var offset = angleOffset.GetRandom();
                vector += Vec2.FromAngle(offset) * distanceOffset.GetRandom();
            }
            return vector;
        }

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    return true;
                case "angleOffset":
                case "angleOffsetMin":
                    angleOffset = reader.ReadAngle();
                    return true;
                case "angleOffsetMax":
                    angleOffset.max = reader.ReadAngle();
                    return true;
                case "relativeAngleOffset":
                case "relativeAngleOffsetMin":
                    relativeAngleOffset = reader.ReadAngle();
                    return true;
                case "relativeAngleOffsetMax":
                    relativeAngleOffset.max = reader.ReadAngle();
                    return true;
                case "distanceOffset":
                case "distanceOffsetMin":
                    distanceOffset = reader.ReadFloat();
                    return true;
                case "distanceOffsetMax":
                    distanceOffset.max = reader.ReadFloat();
                    return true;
                case "relativeDistanceOffset":
                case "relativeDistanceOffsetMin":
                    distanceOffset = reader.ReadFloat();
                    return true;
                case "relativeDistanceOffsetMax":
                    distanceOffset.max = reader.ReadFloat();
                    return true;
            }
            return false;
        }
    }
}
