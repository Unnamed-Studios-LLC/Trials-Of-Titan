using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net;
using Utils.NET.Geometry;

namespace TitanCore.Core
{
    public static class StatusEffectFunctions
    {
        private const float Charmed_Speed = 2.5f;

        private const float Knocked_Back_Speed = 12;

        public static Vec2 GetCharmedPositionVector(Vec2 playerPosition, Vec2 charmerPosition)
        {
            var vector = charmerPosition - playerPosition;
            return vector.ChangeLength(Charmed_Speed);
        }


        public static Vec2 GetKnockedBackPositionVector(Vec2 playerPosition, Vec2 knockerPosition)
        {
            var vector = playerPosition - knockerPosition;
            return vector.ChangeLength(Knocked_Back_Speed);
        }
    }
}
