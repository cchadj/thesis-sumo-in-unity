using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMPAgent : MonoBehaviour {
    private UnityEngine.AI.NavMeshAgent nav;
	// Use this for initialization
	void Start () {
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        nav.destination =GetComponent<GoalManager>().GetNextGoal().position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
