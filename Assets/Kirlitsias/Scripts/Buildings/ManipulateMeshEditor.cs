using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RescaleBuilding))]
public class ManipulateMeshEditor : Editor {


    SerializedProperty scale;
    SerializedProperty update;
    void OnEnable()
    {
        scale = serializedObject.FindProperty("scale");
        update = serializedObject.FindProperty("shouldScale");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(scale);
        //EditorGUILayout.PropertyField(update);
        serializedObject.ApplyModifiedProperties();
        RescaleBuilding myScript = (RescaleBuilding)target;
        if (GUILayout.Button("Rescale Object"))
        {
            myScript.Scale();
        }
        if (GUILayout.Button("Reset Object"))
        {
            myScript.ResetVertices();
        }
        if (GUILayout.Button("Initialize Object"))
        {
            myScript.Initialize();
        }
    }
    void OnDrawGizmos()
    {
        Debug.Log("GIzmaaa");
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        RescaleBuilding myScript = (RescaleBuilding)target;
        //myScript.BuildingGizmos();
    }

    private  void MoveBuilding()
    {
        if (selected != null)
        {
            //Debug.Log("Called");
            Vector3 positionChange;
            positionChange = Handles.PositionHandle(((RescaleBuilding)target)
                .transform.TransformPoint(selected.Center), 
                Quaternion.LookRotation(Vector3.up)) - ((RescaleBuilding)target).transform.TransformPoint(selected.Center);
            if (positionChange != Vector3.zero)
                ((RescaleBuilding)target).MoveBuilding(selected, -positionChange);
            //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
    Quaternion rotationChange;
    private void RotateBuilding()
    {
        if (selected != null)
        {
            //Debug.Log("Called");
            //Quaternion rotationChange;
            rotationChange=Handles.RotationHandle(selected.rotation,
                ((RescaleBuilding)target)
                .transform.TransformPoint(selected.Center));
            //Debug.Log(rotationChange);
            ((RescaleBuilding)target).RotateBuilding(selected, rotationChange);
            //Debug.Log(rotationChange);
            //positionChange = Handles.RotationHandle(((RescaleBuilding)target)
            //    .transform.TransformPoint(selected.Center),
            //    Quaternion.LookRotation(Vector3.up)) - ((RescaleBuilding)target).transform.TransformPoint(selected.Center);
            //if (positionChange != Vector3.zero)
            //((RescaleBuilding)target).MoveBuilding(selected, -positionChange);
            //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
    private Vector3 scaleHandleSize;
    private void ScaleBuilding()
    {
        if (selected != null)
        {
            //Debug.Log("Called");
            Vector3 positionChange;
            scaleHandleSize = Handles.ScaleHandle(scaleHandleSize, ((RescaleBuilding)target)
                .transform.TransformPoint(selected.Center),
                Quaternion.LookRotation(Vector3.up)
                , HandleUtility.GetHandleSize(((RescaleBuilding)target)
                .transform.TransformPoint(selected.Center)));
            ((RescaleBuilding)target).ScaleIndependent(selected, scaleHandleSize);
            //- ((RescaleBuilding)target).transform.TransformPoint(selected.Center);
            //Debug.Log(positionChange);
            //if (positionChange != Vector3.zero)
            //    ((RescaleBuilding)target).MoveBuilding(selected, -positionChange);
            //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }

    private IndependentBuilding selected;

    private int toolState;
    private const int ROTATE = 0;
    private const int MOVE = 1;
    private const int SCALE = 2;


    private void Mode()
    {
        if (Event.current.type != EventType.KeyDown)
            return;
        if (Event.current.keyCode==(KeyCode.E))
        {
            toolState = ROTATE;
            rotationChange = Quaternion.LookRotation(Vector3.up);
        }
        else if (Event.current.keyCode == (KeyCode.W))
        {
            toolState = MOVE;
        }
        else if (Event.current.keyCode == (KeyCode.R))
        {
            toolState = SCALE;
            scaleHandleSize = new Vector3(1, 1, 1);
        }
    }

    void CallMode()
    {
        //Debug.Log("STate:" + toolState);
        switch (toolState)
        {
            case ROTATE:
                RotateBuilding();
                break;
            case MOVE:
                MoveBuilding();
                break;
            case SCALE:
                ScaleBuilding();
                break;
            default:
                break;
        }
    }

    void OnSceneGUI()
    {
        RescaleBuilding t = (RescaleBuilding)target;
        Handles.color = Handles.xAxisColor;
        Mode();
        CallMode();
        //MoveBuilding();
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (t == null )
            return;
        if (Event.current.type == EventType.MouseDown)
        {
            if (t.BuildingTree.Count == 0)
            {
                t.Initialize();
            }
            float[] tmp = new float[2];
            KdTree.KdTreeNode<float, IndependentBuilding>[] nodes;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 10000000.0f))
            {

                if (hit.transform.gameObject.tag != "building")
                    return;
                //Debug.Log(hit.point);
                new GameObject().transform.position = hit.point;
                hit.point=
                ((RescaleBuilding)target)
                .transform.InverseTransformPoint(hit.point);
                tmp[0] = hit.point.x;
                tmp[1] = hit.point.z;
                nodes=t.BuildingTree.GetNearestNeighbours(tmp, 1);
                
                selected = nodes[0].Value;
                //Debug.Log("Center is:"+selected.Center);
                //nodes[0].Value.
                //Debug.Log(Event.current.mousePosition);
                //Vector3 newTilePosition = hit.point;
                //Instantiate(newTile, newTilePosition, Quaternion.identity);
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
        }
   
        return;
        
        //Debug.Log("Called");
        // grab the center of the parent
        //Vector3 center = t.transform.position;
        //// iterate over game objects added to the array...
        //int i = 0;
        //Handles.color = Handles.xAxisColor;
        //foreach (var item in t.buildings)
        //{
        //    //Debug.Log("Yep");
        //    //Debug.Log("calledd");
        //    item.FindCenter();
        //    //Handles.ArrowHandleCap(i,t.transform.TransformPoint(item.Center), Quaternion.LookRotation(Vector3.up), 10,EventType.Repaint);

        //    positionChange=Handles.PositionHandle(t.transform.TransformPoint(item.Center), Quaternion.LookRotation(Vector3.up))-t.transform.TransformPoint(item.Center);
        //    if(positionChange!=Vector3.zero)
        //        t.MoveBuilding(item, positionChange);
        //    i++;
        //    //transform.TransformPoint(mesh.vertices[0]
        //}
        //for (int i = 0; i < t.buildings.Count; i++)
        //{
        //    // ... and draw a line between them
        //    if (t.GameObjects[i] != null)
        //        Handles.DrawLine(center, t.GameObjects[i].transform.position);
        //}
    }

}

public class SelectedBuilding {

    private IndependentBuilding building;
    private bool selected = false;

    public SelectedBuilding(IndependentBuilding building)
    {
        this.building = building;
    }
    public bool Selected
    {
        get
        {
            return selected;
        }
        set
        {
            selected = value;
        }
    }
}


//public class MyScriptGizmoDrawer
//{
//    [DrawGizmo(GizmoType.Pickable | GizmoType.Active)]
//    static void DrawGizmoForMyScript(RescaleBuilding scr, GizmoType gizmoType)
//    {
//        Vector3 position = scr.transform.position;
//        //Debug.Log("Not calledd,nop");
//        if (scr.buildings.Count == 0)
//        {
//            return;
//        }

//        foreach (var item in scr.buildings)
//        {
//            //Debug.Log("calledd");
//            item.FindCenter();
//            Gizmos.DrawCube(scr.transform.TransformPoint(item.Center), new Vector3(5, 5, 5));
//            //transform.TransformPoint(mesh.vertices[0]
//        }
//        //Gizmos.DrawIcon(position, "MyScript Gizmo.tiff");
//    }
//}
