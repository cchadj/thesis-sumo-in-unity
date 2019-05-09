using RiseProject.Kirlitsias;
using UnityEngine;

public class GraphCarSpawner : MonoBehaviour {

    public GameObject prefab;
    private delegate SimpleGraphRoadNetwork.Graph GetGraph();
    //private  SimpleGraphRoadNetwork.Graph graph;
    GetGraph getGraph;
    // Use this for initialization
    void Start () {
        Debug.Log(Vector3.Dot(Vector3.up, Vector3.up));
        getGraph = () => GameObject.Find("Graph").GetComponent<SimpleGraphRoadNetwork>().Graph1;      
	}
	
    private void SpawnACarAtEachJunction()
    {     
        GameObject tmp;
        foreach(SimpleGraphRoadNetwork.Node node in getGraph().Nodes)
        {
            tmp=Instantiate(prefab, node.Position, new Quaternion());
            tmp.GetComponent<CarLogic>().InitializeCar(node);
            ///Use event to trigger another entity to start finding paths or something.
            ///Car needs to plan whith some algorithm.
        }
    }

	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SpawnACarAtEachJunction();
        }

	}
}
