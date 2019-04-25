using UnityEngine;

namespace RiseProject.Tomis.VehicleControl
{ 
    [RequireComponent(typeof(DynamicWaypointCircuit))]
    public class DynamicSumoWaypointProgressTracker : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private float distanceFromTarget;
        [SerializeField]
        private int _jumpWayPoints = 1;

        private DynamicWaypointCircuit m_dynamicCircuit;
        protected  void Start()
        {

            m_dynamicCircuit = GetComponent<DynamicWaypointCircuit>();
        }


        private DynamicWaypointCircuit.WaypointData _targetData;

        [SerializeField] private float _pointToPointThreshold;
        [SerializeField] private Transform target;

        private int _progressNum;

        public DynamicWaypointCircuit.WaypointData TargetData { get => _targetData; set => _targetData = value; }

        // Update is called once per frame
        protected  void Update()
        {
            if (m_dynamicCircuit.WaypointsData == null || m_dynamicCircuit.WaypointsData.Length == 0)
                return;
            
            // Calculate distance only on XZ plane in our case.
            Vector3 targetPos = new Vector3(target.position.x, 0f, target.position.z);
            Vector3 transformPos = new Vector3(transform.position.x, 0f, transform.position.z);

            distanceFromTarget = Vector3.Distance(targetPos, transformPos);

            if (distanceFromTarget < _pointToPointThreshold)
            {
                _progressNum = (_progressNum + _jumpWayPoints) % m_dynamicCircuit.WaypointsData.Length;
            }

            TargetData = m_dynamicCircuit.WaypointsData[_progressNum];

            target.position = TargetData.position;
            target.rotation = TargetData.rotation;
        }

        protected void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(target.position, target.position + target.forward);
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(target.position, Vector3.one * 3);
                Gizmos.color = Color.grey;

            }
        }
    }
}