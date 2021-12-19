using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.Algorithms;
using Utils.NET.Geometry;
using World.Logic.Reader;
using World.Logic.States;
using World.Map;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class PetFollowValue
    {
        public float nextLineCheck;
    }

    public class PetFollow : LocationEnforcement<PetFollowValue>
    {
        protected override void InitValue(Entity enemy, out PetFollowValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new PetFollowValue();
        }

        protected override bool ShouldEnforce(Entity entity, ref PetFollowValue obj, ref StateContext context, ref WorldTime time, out Vec2 vector)
        {
            if (entity.world == null || !(entity is Pet pet) ||
                !pet.world.objects.TryGetPlayer(pet.ownerAccountId, out var owner))
            {
                vector = Vec2.zero;
                return false;
            }

            vector = owner.position.Value - pet.position.Value;
            var length = vector.Length;
            if (length > 24)
            {
                pet.MoveTo(pet.position.Value + vector.ChangeLength(length - 24, length));
                vector = Vec2.zero;
                return false;
            }

            if (obj.nextLineCheck < time.totalTime && !CheckLine(entity.world, pet.position.Value.ToInt2(), owner.position.Value.ToInt2()))
            {
                obj.nextLineCheck = (float)time.totalTime + 5;

                var line = Bresenham.Line(pet.position.Value.ToInt2(), owner.position.Value.ToInt2()).ToList();
                line.Reverse();
                var distance = (int)length;
                while (line.Count > 0)
                {
                    bool failed = false;
                    foreach (var point in line)
                        if (pet.world.tiles.GetCollisionType(point.x, point.y).HasFlag(CollisionType.Wall))
                        {
                            failed = true;
                            break;
                        }

                    if (failed)
                    {
                        line.RemoveAt(line.Count - 1);
                        continue;
                    }

                    pet.MoveTo(line[line.Count - 1].ToVec2() + 0.5f);
                    vector = Vec2.zero;
                    return false;
                }

                pet.MoveTo(owner.position.Value);
                vector = Vec2.zero;
                return false;
            }

            if (length > distance)
            {
                vector = vector.ChangeLength(1, length);
                return true;
            }

            return false;
        }

        private bool CheckLine(World world, Int2 from, Int2 to)
        {
            foreach (var point in Bresenham.Line(from, to))
            {
                if (world.tiles.GetCollisionType(point.x, point.y).HasFlag(CollisionType.Wall))
                    return false;
            }
            return true;
        }
    }
}
