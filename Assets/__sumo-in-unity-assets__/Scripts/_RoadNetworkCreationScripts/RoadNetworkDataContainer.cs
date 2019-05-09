using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadNetworkDataContainer : MonoBehaviour {

    [SerializeField]
    private EdgeIDList _edgeIDList;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

[Serializable]
public class EdgeIDList : List<string>
{
    

}