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
					if ( !gameObject.transform.GetEnumerator().MoveNext() &&  gameObject.name == name)
						AddGameObjectRelativeTo(gameObject, objectToPlace, relativePositionAndOrientation,
							tranformToPlaceNewObjectsUnder);
			

	    if(gameObjects != null)
			foreach (var gameObject in gameObjects)
				AddGameObjectRelativeTo(gameObject, objectToPlace, relativePositionAndOrientation,
					tranformToPlaceNewObjectsUnder);

	    if(parentOfObjects)
	    foreach (GameObject child in parentOfObjects)
		    AddGameObjectRelativeTo(gameObject, objectToPlace, relativePositionAndOrientation,
			    tranformToPlaceNewObjectsUnder);

	    
    }

    private static void AddGameObjectRelativeTo(
	    GameObject o,
	    GameObject objToPlace,
	    Transform relativeTransform,
	    Transform parentToPlaceNewObjectsUnder = null)
    {
	    var newGameObject =  (GameObject)PrefabUtility.InstantiatePrefab(objToPlace);
	    newGameObject.transform.parent = o.transform;

	    newGameObject.transform.localPosition = relativeTransform.position;
	    newGameObject.transform.localRotation = relativeTransform.rotation;

	    newGameObject.transform.parent = parentToPlaceNewObjectsUnder;
    }
}
#endif

