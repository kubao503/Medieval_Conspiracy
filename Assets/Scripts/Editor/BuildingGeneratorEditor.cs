using UnityEngine;
using UnityEditor;
using PathCreation.Examples;

[CustomEditor(typeof(BuildingGenerator), true)]
public class BuildingGeneratorEditor : PathSceneToolEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Random Building Generation"))
        {
            ((BuildingGenerator)target).NewSeed();
        }
    }
}

