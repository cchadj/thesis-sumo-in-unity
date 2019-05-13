using System;
using UnityQuery;
using UnityEngine;
using UnityEditor;

public class DisplayRenderer : MonoBehaviour
{

    [SerializeField] private bool replaceAndSimplifyMeshes;
    [ReadOnly] public bool renderersEnabled = true;

    private void Start()
    {

        if(replaceAndSimplifyMeshes)
            foreach (var child in gameObject.GetDescendantsAndSelf())
            {
                var laneInCameraFrustum = child.GetComponent<LaneInCameraFrustum>();
                if (laneInCameraFrustum)
                    laneInCameraFrustum.ReplaceMesh();
            }
    }

    // Update is called once per frame
    public void DisplayRenders(bool display)
    {

        foreach (var child in gameObject.GetDescendantsAndSelf())
        {
            var r = child.GetComponent<Renderer>();
            if (r)
                r.enabled = display;
        }

        renderersEnabled = display;
    }
    
    
    
}

#if UNITY_EDITOR

[CustomEditor(typeof(DisplayRenderer))]
public class DisplayRendererEditor : Editor
{
    private DisplayRenderer Display => (DisplayRenderer) target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button((Display.renderersEnabled ? "Disable Renderers" : "Enable Renderers")))
            Display.DisplayRenders(!Display.renderersEnabled);
    }
}

#endif
