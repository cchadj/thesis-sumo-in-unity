using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using DSA.DataStructures.Graphs;
public class GraphFromXml : MonoBehaviour {



    public class Node : System.IComparable<Node>
    {
        
        private Vector3 position;


        public Node()
        {

        }
        public Node(Vector3 position)
        {
            this.position = position;
        }

        public int CompareTo(Node other)
        {
            if ((other.position - Vector3.zero).magnitude < (position - Vector3.zero).magnitude)
                return 1;
            else return -1;
        }
    }


    /// <summary>
    /// This class is used to represent open street map edge data for the
    /// QuickGraph library.
    /// </summary>
    public class OemEdge : System.IComparable<OemEdge>
    {
        /// <summary>
        /// Denotes the starting junction of this edge.
        /// </summary>
        private Node source;
        /// <summary>
        /// Denotes the ending junction of this edge
        /// </summary>
        private Node target;

        /// <summary>
        /// These parts denote the individual parts of an edge.In the net.xml file used
        /// an edge can have more than two points, usually due to being a curved road.
        /// </summary>
        private List<Vector3> edgeIndividualParts;
        private float speed;
        private float length;

        // Something that will keep current cars on this edge and will give various capabilities
        // to the programmer like, time to collision, traffic density etc.Things that can be used to
        //create an intelligent agent.

        public OemEdge()
        {

        }

        public OemEdge(Node source, Node target)
        {
            this.source = source;
            this.target = target;
        }
        public OemEdge(Node source, Node target, List<Vector3> edgeIndividualParts)
        {
            this.source = source;
            this.target = target;
            this.edgeIndividualParts = edgeIndividualParts;
        }
        private void SetSource(Node source)
        {
            this.source = source;
        }
        private void SetTarget(Node target)
        {
            this.target = target;
        }

        public int CompareTo(OemEdge other)
        {
            if (length < other.length)
                return -1;
            if (length == other.length)
                return 0;
            return 1;
        }

        public Node Source
        {
            get
            {
                return source;
            }
        }

        public Node Target => target;
    }
    // Use this for initialization
    void Start () {
        DirectedWeightedALGraph<Node, OemEdge> oNo = new DirectedWeightedALGraph<Node, OemEdge>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}



}

