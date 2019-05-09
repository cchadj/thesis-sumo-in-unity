using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CallSetterAttribute))]
class CallSetterPropertyDrawer : PropertyDrawer
{

    private CallSetterAttribute SetterAttribute {  get { return attribute as CallSetterAttribute; } }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //  base.OnGUI(position, property, label);
        EditorGUI.PropertyField(position, property, label, true);


        var targetObject = property.serializedObject.targetObject;

        var parrentClassType = targetObject.GetType();

        var propertyInfo = parrentClassType.GetProperty(SetterAttribute.PropertySetterName);

        var value = fieldInfo.GetValue(targetObject);
        propertyInfo.SetValue(targetObject, value);
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }

}