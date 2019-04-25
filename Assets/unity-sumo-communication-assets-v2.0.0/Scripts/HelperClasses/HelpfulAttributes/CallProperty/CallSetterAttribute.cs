using UnityEngine;

/// <summary>
/// Attribute to call setter when changing via the editor
/// </summary>
public class CallSetterAttribute : PropertyAttribute
{
    public string PropertySetterName { get; }
    public CallSetterAttribute(string propertySetterName)
    {
        PropertySetterName = propertySetterName;
    }
}
