using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GateGenerator))]
public class GateGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GateGenerator myScript = (GateGenerator)target;
        if (GUILayout.Button("Generate"))
        {
            myScript.Generate();
        }

        if (GUILayout.Button("Generate Rand Seed"))
        {
            myScript.Generate(true);
        }

        if (GUILayout.Button("Export Shape"))
        {
            myScript.ExportShape();
        }
    }
}
