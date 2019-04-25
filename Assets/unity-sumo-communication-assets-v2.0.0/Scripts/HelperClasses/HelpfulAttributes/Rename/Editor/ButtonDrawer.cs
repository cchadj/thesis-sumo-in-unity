
using UnityEditor;
using UnityEngine;
using Supyrb;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonDrawer : PropertyDrawer
{

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        if (GUILayout.Button("hello"))
        {
            
        }
    }
}