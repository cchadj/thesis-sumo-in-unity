using System;
using UnityEditor;
using UnityEngine;

/// <summary>
///   <para>The ContextMenu attribute allows you to add commands to the context menu.</para>
/// </summary>
public class ButtonAttribute : Attribute
{
    public readonly string name;

    public ButtonAttribute( string name )
    {
        this.name = name ;
    }
}


         
         