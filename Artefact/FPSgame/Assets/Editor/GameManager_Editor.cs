using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManager_Editor: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Get Model"))
        {
            (target as GameManager).GetModel();
        }

        if(GUILayout.Button("Analyse"))
        {
            (target as GameManager).Analyse();
        }

        if(GUILayout.Button("Test data"))
        {
            (target as GameManager).Test();
        }
    }

}
