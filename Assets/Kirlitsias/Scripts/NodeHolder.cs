using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RiseProject.Kirlitsias
{



    public class NodeHolder : MonoBehaviour
    {
        [SerializeField] public SimpleGraphRoadNetwork.Node MYNODEREEE;

        public GameObject LanePartPrefab;

        public bool DRAW = false;
        public bool RemoveEdge = false;
        public bool ModifyLane = false;
        public bool SaveOnDestroy = false;

        public int laneToModify = 0;
        public int edgeToModify = 0;
        public bool bound = false;

        private void FixedUpdate()
        {
            if (DRAW)
            {
                MYNODEREEE.DrawRaysForMe();
                if (RemoveEdge)
                {
                    MYNODEREEE.OutEdges.RemoveAt(0);
                    RemoveEdge = false;
                }

                if (ModifyLane)
                {
                    SpawnLane();
                }
                else
                {
                    DestroyChildren();
                }
            }

            if (true)
            {

                for (int i = 0; i < MYNODEREEE.pointsAroundNode.Count - 1; i++)
                {
                    Debug.DrawRay(MYNODEREEE.pointsAroundNode[i].vector,
                        MYNODEREEE.pointsAroundNode[i + 1].vector - MYNODEREEE.pointsAroundNode[i].vector, Color.cyan);
                }

                //Debug.DrawRay(MYNODEREEE.pointsAroundNode[MYNODEREEE.pointsAroundNode.Count - 1].vector, -MYNODEREEE.pointsAroundNode[MYNODEREEE.pointsAroundNode.Count - 1].vector + MYNODEREEE.pointsAroundNode[0].vector, Color.cyan);
            }
        }

        private void Start()
        {
            //MYNODEREEE.CreateABoundedArea();

        }


        private bool laneIsSpawned = false;

        private void SpawnLane()
        {
            GameObject laneInstance;
            SimpleGraphRoadNetwork.Lane lane;
            SimpleGraphRoadNetwork.Edge edge;
            int counter = 0;
            if (laneIsSpawned) return;
            edge = MYNODEREEE.OutEdges[edgeToModify % MYNODEREEE.OutEdges.Count];
            lane = edge.Lanes[laneToModify % edge.Lanes.Count];
            counter = 0;
            foreach (Vector3 part in lane.EdgeParts)
            {
                laneInstance = Instantiate(LanePartPrefab, part, new Quaternion());
                laneInstance.GetComponent<LaneEdit>().lane = lane;
                laneInstance.GetComponent<LaneEdit>().LanePartindex = counter++;
                laneInstance.transform.parent = transform;

            }

            laneIsSpawned = true;
        }

        private void DestroyChildren()
        {
            laneIsSpawned = false;
            foreach (Transform child in transform)
            {

                if (child == transform)
                    continue;
                if (SaveOnDestroy)
                    child.GetComponent<LaneEdit>().lane.SaveLaneShape();
                Destroy(child.gameObject);
            }
        }


    }

    public class AngleVector3 : System.IComparable<AngleVector3>
    {
        public float angle = 0;
        public Vector3 vector;
        public int sidewalk = 0;
        public int sidewalkIndexPart = 0;
        public SimpleGraphRoadNetwork.Edge edge;

        public int CompareTo(AngleVector3 other)
        {
            if (angle < other.angle)
                return -1;
            if (angle > other.angle)
                return 1;
            return 0;
        }

        public AngleVector3(Vector3 vector, int sidewalk, int part, SimpleGraphRoadNetwork.Edge edge)
        {
            this.vector = vector;
            this.sidewalkIndexPart = part;
            this.sidewalk = sidewalk;
            this.edge = edge;
        }

        public void CaclulateAngle(Vector3 reference)
        {
            angle = Vector3.SignedAngle((reference + Vector3.forward - reference), (vector - reference), Vector3.up);
        }
    }
}