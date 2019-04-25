using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadNetworkRenderer))]
public class RoadNetworkRendererEditor : Editor
{
    GameObject renderedRoadNetwork = null;

    private static GameObject[] FindGameObjectsInLayer(int layer)
    {
        var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        var goList = new List<GameObject>();
        for (int i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == layer)
            {
                goList.Add(goArray[i]);
            }
        }
        if (goList.Count == 0)
        {
            return null;
        }
        return goList.ToArray();
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RoadNetworkRenderer rendererScript = (RoadNetworkRenderer)target;

        if (GUILayout.Button("Render Network", EditorStyles.miniButton))
        {
            GameObject[] generatedNetworks = GameObject.FindGameObjectsWithTag("GeneratedRoadNetwork");
            if (generatedNetworks != null || generatedNetworks.Length != 0)
                foreach (GameObject g in generatedNetworks)
                {
                    Debug.Log("Destroying old road network with name : " + g.name);
                    GameObject.DestroyImmediate(g, false);
                    renderedRoadNetwork = null;
                }
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("GeneratedDictionaries"))
                {
                    Debug.Log("Destroying old road network with name : " + g.name);
                    GameObject.DestroyImmediate(g, false);
                }
            renderedRoadNetwork = rendererScript.RenderNetwork();
            Debug.Log(renderedRoadNetwork.ToString());
        }

        if (GUILayout.Button("Clean Height Buffer", EditorStyles.miniButton))
        {
            throw new NotImplementedException();
           // Toolbox.Instance.CleanHeightList();
           // Debug.Log("Cleaned Height Buffer");
        }

        if (GUILayout.Button("Fill Height Buffer", EditorStyles.miniButton))
        {
            List<GameObject> heightBufferObjects = rendererScript.heightBufferObjects;
            foreach (GameObject o in heightBufferObjects)
            {
                HeightBufferCreator script = o.GetComponent<HeightBufferCreator>();
                if (script != null)
                    script.FillHeightBuffer();

                Transform[] allChildren = o.GetComponentsInChildren<Transform>();

                foreach (Transform c in allChildren)
                {
                    script = c.gameObject.GetComponent<HeightBufferCreator>();
                    if (script != null)
                        script.FillHeightBuffer();
                }
            }
            Debug.Log("Height Buffer filled");
        }

        if (GUILayout.Button("Print Height Buffer", EditorStyles.miniButton))
        {
            throw new NotImplementedException();
            //Toolbox.Instance.PrintHeightBufferList();
        }

        if (GUILayout.Button(
            new GUIContent()
            {
                text = "Match height with height buffer heights",
                tooltip = "Use ray casting to place the vehicle road network at the same height as Andreas Road Network"
            }, EditorStyles.miniButton))
        {
            Transform roadnetworkParentTransform;
            if (renderedRoadNetwork == null)
            {
                renderedRoadNetwork = GameObject.FindGameObjectWithTag("GeneratedRoadNetwork");
            }

            if (renderedRoadNetwork != null)
            {
                Debug.Log("Wake up sheeple");
                roadnetworkParentTransform = renderedRoadNetwork.transform;
                foreach (Transform child in roadnetworkParentTransform)
                {
                    MeshHeightMatcher mhm = child.gameObject.GetComponent<MeshHeightMatcher>();
                    mhm.MatchHeight();
                }
            }
        }

        if (GUILayout.Button(new GUIContent()
        {
            text = "Stop Showing Rendered network",
            tooltip = "Use this for optimisation AFTER matching height"
        }, EditorStyles.miniButton))
        {
            if (renderedRoadNetwork == null)
                renderedRoadNetwork = GameObject.FindGameObjectWithTag("GeneratedRoadNetwork");

            if (renderedRoadNetwork != null)
            {
                foreach (Transform c in renderedRoadNetwork.transform)
                {
                    GameObject child = c.gameObject;
                    (child.GetComponent<MeshRenderer>()).enabled = false;
                    GameObject.DestroyImmediate(child.GetComponent<MeshFilter>());
                }

            }
        }

        /***********************************************************************************************************/
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

    }
}


