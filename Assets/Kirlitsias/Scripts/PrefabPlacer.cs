#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityQuery;

public class PrefabPlacer : MonoBehaviour
{

    [Header("Place Prefabs Relative to")]
    public List<string> objectsWithNames;
    public List<GameObject> gameObjects;
    public Transform parentOfObjects;
	
    [Header("Prefabs to place")]
    public GameObject objectToPlace;
    public Transform relativePositionAndOrientation;
    public Transform tranformToPlaceNewObjectsUnder;
	
    
    [ContextMenu("Place Prefabs")]
    // Use this for initialization
    public void PlacePrefabs ()
    {

	    if (objectsWithNames != null) 
			foreach (var name in objectsWithNames)
				foreach (var gameObject in (GameObject[]) FindObjectsOfType(typeof(GameObject)))
				{
					// The second part is because when the localPosition is (0,0,0) then the object is just a placeholder
					// and does not represent a gameobject with a renderer
					if ( gameObject.name == name && gameObject.transform.localPosition != Vector3.zero)
						Undo.RegisterCreatedObjectUndo(AddGameObjectRelativeTo(gameObject, objectToPlace, relativePositionAndOrientation,
							tranformToPlaceNewObjectsUnder),"placed prefab at scene");
				}

	    
	    if(gameObjects != null)
			foreach (var gameObject in gameObjects)
				AddGameObjectRelativeTo(gameObject, objectToPlace, relativePositionAndOrientation,
					tranformToPlaceNewObjectsUnder);

	    
	    if(parentOfObjects)
	    foreach (GameObject child in parentOfObjects)
		    AddGameObjectRelativeTo(gameObject, objectToPlace, relativePositionAndOrientation,
			    tranformToPlaceNewObjectsUnder);

	    
    }

    private GameObject AddGameObjectRelativeTo(
	    GameObject o,
	    GameObject objToPlace,
	    Transform relativeTransform,
	    Transform parentToPlaceNewObjectsUnder = null)
    {
	    var newGameObject =  (GameObject)PrefabUtility.InstantiatePrefab(objToPlace);

	    newGameObject.transform.position = o.transform.TransformPoint(relativeTransform.position);
	    newGameObject.transform.rotation = o.transform.localRotation * relativeTransform.rotation;

	    newGameObject.transform.parent = parentToPlaceNewObjectsUnder;
	    return newGameObject;
    }
}
#endif

