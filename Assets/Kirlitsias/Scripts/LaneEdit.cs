using System.Collections;
using System.Collections.Generic;
using RiseProject.Kirlitsias;
using UnityEngine;

public class LaneEdit : MonoBehaviour {

    /// <summary>
    /// Relation to the 
    /// </summary>
    public SimpleGraphRoadNetwork.Lane lane;
    private int lanePartindex;

    public int LanePartindex
    {
        get
        {
            return lanePartindex;
        }

        set
        {
            lanePartindex = value;
        }
    }

    // Use this for initialization


    // Update is called once per frame
    void FixedUpdate () {
        lane.EdgeParts[lanePartindex] = transform.position;
	}
}
