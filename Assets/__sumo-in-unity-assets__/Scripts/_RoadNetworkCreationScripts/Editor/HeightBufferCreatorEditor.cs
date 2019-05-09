using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HeightBufferCreator))]
public class HeightBufferCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HeightBufferCreator HeightBufferScript = (HeightBufferCreator)target;
        if (GUILayout.Button("Fill Height Buffer", EditorStyles.miniButton))
        {
            HeightBufferScript.FillHeightBuffer();
        }

        if (GUILayout.Button("Print Height Buffer List", EditorStyles.miniButton))
        {
            // Toolbox.Instance.PrintHeightBufferList();
        }

        if (GUILayout.Button("Clean Height Buffer List", EditorStyles.miniButton))
        {
            // Toolbox.Instance.CleanHeightList();
        }
    }
}