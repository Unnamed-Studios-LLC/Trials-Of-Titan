using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldCreator))]
public class WorldCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldCreator myScript = (WorldCreator)target;
        if (GUILayout.Button("Save"))
        {
            string filePath = EditorUtility.SaveFilePanel("Save Map", "", "", "mef");
            if (string.IsNullOrWhiteSpace(filePath)) return;
            File.WriteAllBytes(filePath, myScript.Export(2048));
        }
    }
}