using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data.Map;
using UnityEngine;
using Utils.NET.Geometry;
using Utils.NET.Logging;

public class LogBridge : MonoBehaviour
{
    private void Awake()
    {
        Log.backupWrite = Write;

        //Debug.Log(typeof(TileInfo).Assembly.FullName);
        //Debug.Log(typeof(Vec2).Assembly.FullName);
    }

    private static StringBuilder builder = new StringBuilder();

    private static void Write(object o)
    {
        var t = o.ToString();
        var split = t.Split('\n');
        if (split.Length == 0) return;
        for (int i = 0; i < split.Length - 1; i++)
        {
            builder.Append(split[i]);
            Debug.Log(builder.ToString());
            builder.Clear();
        }
        builder.Append(split[split.Length - 1]);
    }
}