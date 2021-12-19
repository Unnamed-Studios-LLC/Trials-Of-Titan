using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(TitanButton))]
public class TitanButtonEditor : ButtonEditor
{
    public override void OnInspectorGUI()
    {
        var button = (TitanButton)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("content"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("textLabel"));

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
