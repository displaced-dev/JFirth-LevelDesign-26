using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using LevelDesign.Data;

[CustomEditor(typeof(ProgressionDataSO))]
public class ProgressionDataSOEditor : Editor
{
    public override bool RequiresConstantRepaint() => Application.isPlaying;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime | NOT SAVED!!!", EditorStyles.boldLabel);

        var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var f in fields)
        {
            if(!f.IsNotSerialized) continue;

            string label = ObjectNames.NicifyVariableName(f.Name);
            object value = f.GetValue(target);

            if(f.FieldType == typeof(int)) {
                f.SetValue(target, EditorGUILayout.IntField(label, (int)value));
            }
            else if(f.FieldType == typeof(bool)) {
                f.SetValue(target, EditorGUILayout.Toggle(label, (bool)value));
            }
            else if(f.FieldType == typeof(float)) {
                f.SetValue(target, EditorGUILayout.FloatField(label, (float)value));
            }
            else if(f.FieldType == typeof(string)) {
                f.SetValue(target, EditorGUILayout.TextField(label, (string)value));
            }
            else{
                EditorGUILayout.LabelField(label, value?.ToString() ?? "null");
            }
        }
    }
}