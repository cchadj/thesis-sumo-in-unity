using RiseProject.Tomis.VehicleControl;
using UnityEditor;

public class MeantToBeAbstractEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("DO NOT attach this class. This class was meant to" +
            " be abstract but GetComponent can not be used for abstract classes ", MessageType.Error);
    }
}

[CustomEditor(typeof(VehicleMover))]
public class VehicleMoverEditor : MeantToBeAbstractEditor { }

[CustomEditor(typeof(VehicleMoverByInterpolation))]
public class VehicleMoverByInterpolationEditor : MeantToBeAbstractEditor { }