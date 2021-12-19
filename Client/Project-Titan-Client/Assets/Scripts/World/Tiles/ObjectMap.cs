using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.NET.Geometry;

public class ObjectMap
{
    private ushort[,] objectData;

    private readonly Dictionary<ulong, WorldObject> objects = new Dictionary<ulong, WorldObject>();

    private World world;

    private int chunkSize;

    private Int2 focus = new Int2(-1, -1);

    public ObjectMap(World world, int chunkSize)
    {
        this.world = world;
    }

    public void Initialize(int width, int height)
    {
        objectData = new ushort[width, height];

        foreach (var obj in objects)
        {
            world.gameManager.objectManager.ReturnObject(obj.Value);
        }
        objects.Clear();
    }

    public void SetFocus()
    {

    }
}