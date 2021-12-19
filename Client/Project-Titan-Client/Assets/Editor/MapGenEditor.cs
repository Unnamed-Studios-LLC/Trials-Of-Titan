using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGen))]
public class MapGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        /*
        MapGen myScript = (MapGen)target;
        if (GUILayout.Button("Generate"))
        {
            myScript.Generate();
        }

        if (GUILayout.Button("Generate Random"))
        {
            myScript.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            myScript.Generate();
        }

        if (GUILayout.Button("Export Config"))
        {

        }

        if (GUILayout.Button("Import Config"))
        {

        }
        */
    }
}
