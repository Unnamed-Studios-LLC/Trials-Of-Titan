using System;

namespace WorldGen
{
    public static class WorldGen
    {
        public static World Generate(int width, int height, int seed, int relaxations, int pointCount)
        {
            World world;
            do
            {
                try
                {
                    world = new World(width, height, seed);
                    world.Generate(pointCount, relaxations);
                }
                catch
                {
                    world = null;
                }
            } while (world == null);
            return world;
        }
    }
}
