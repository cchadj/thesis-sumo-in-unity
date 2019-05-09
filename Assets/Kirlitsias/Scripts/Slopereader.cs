using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slopereader : MonoBehaviour {

    TerrainData ter;
    float height;
    float widht;
    // Use this for initialization
	void Start () {
        ter = GetComponent<Terrain>().terrainData;
        Debug.Log(ter.detailWidth);
        Debug.Log(ter.detailHeight);
        //for (int i = 0; i < ter.detailHeight; i+=10)
        //{
        //    for (int j = 0; j < ter.detailHeight; j+=10)
        //    {
        //        Debug.Log(ter.GetSteepness(i/(float)ter.detailHeight, j / (float)ter.detailHeight));
        //    }
        //}
        
        //ter.get
    }
	// Update is called once per frame
	void Update () {
		
	}
}
