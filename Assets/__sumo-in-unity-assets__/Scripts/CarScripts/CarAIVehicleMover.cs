using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;

using UnityStandardAssets.Vehicles.Car;

namespace RiseProject.Tomis.VehicleControl
{
    [RequireComponent(typeof(CarAIControl), typeof(CarController))]
    [RequireComponent(typeof(DynamicWaypointCircuit), typeof(DynamicSumoWaypointProgressTracker))]
    public class CarAIVehicleMover : VehicleMover
    {
        [SerializeField] private DynamicWaypointCircuit m_waypointCircuit;
        [SerializeField] private DynamicSumoWaypointProgressTracker m_waypointProgressTracker;

        private CarAIControl m_carAIControl;
        private CarController m_carController;

        [SerializeField] private int _addEveryNumberOfSteps = 1;

        protected override void Awake()
        {
            WantToSubscribeToVehicle = true;
            NumberOfStepsUsedForPathCalculation = 0;
            numberOfLagSteps = 10;

            m_carAIControl = GetComponent<CarAIControl>();
            m_carAIControl.enabled = false;
            m_waypointCircuit = GetComponent<DynamicWaypointCircuit>();
            m_waypointCircuit.enabled = false;
            m_waypointProgressTracker = GetComponent<DynamicSumoWaypointProgressTracker>();
            m_waypointProgressTracker.enabled = false;
            m_carController = GetComponent<CarController>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_carAIControl.enabled = true;
            /* Start driving after _lag times of steps pass. */
            m_carAIControl.Driving = false;
            m_waypointCircuit.enabled = true;
            m_waypointProgressTracker.enabled = true;
            //m_waypointCircuit.AddWaypoint(Vehicle.Position2D, Vehicle.Angle);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            m_carAIControl.enabled = false;
            m_carAIControl.Driving = false;
            m_waypointCircuit.enabled = false;
            m_waypointProgressTracker.enabled = false;
        }

        private int _curStep = 0;

        private float _expectedSpeed = 0f;
        private float _actualSpeed;
        private float _sumoSpeed;

        private DynamicWaypointCircuit.WaypointData _prevWaypoint;
        /// <summary>
        /// Consider Moving after 2 or three steps occur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Vehicle_VehicleTransformChanged(object sender, Vehicle.VehicleArgs e)
        {
            _curStep++;
            _expectedSpeed = SumoVehicle.Speed;

            if (m_waypointCircuit.WaypointsData.Length > numberOfLagSteps)
            {
                m_carAIControl.Driving = true;
                _expectedSpeed =  Vector3.Distance(SumoVehicle.PrevPosition2D, SumoVehicle.Position) / SharedVehicleData.SimulationStepLength;
                m_carController.MaxSpeed = m_waypointProgressTracker.TargetData.nextWaypoint.speed;
            }

            if (_curStep != 0 && (_curStep % _addEveryNumberOfSteps == 0))
            {
                DynamicWaypointCircuit.WaypointData curWaypoint = m_waypointCircuit.AddWaypoint(SumoVehicle.Position, SumoVehicle.Angle, _expectedSpeed, SumoVehicle.Speed == 0);
                if(_prevWaypoint != null)
                    _prevWaypoint.nextWaypoint = curWaypoint;
                _prevWaypoint = curWaypoint;
            }
                
        }
#if UNITY_EDITOR
        private Rect _windowRect = new Rect(20f, 20f, 200f, 1f);
        private void OnGUI()
        {
            
            GUILayout.Window(0, _windowRect, DrawWindow, $"{SumoVehicle.ID} Speed");
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.Label($"Sumo Speed: {SumoVehicle.Speed} m/s");
            GUILayout.Label($"Expected Speed: {_expectedSpeed} m/s");
            GUILayout.Label($"Actual Speed: {m_carController.CurrentSpeed}");
        }

        static public void DrawString(string text, Vector3 worldPos, Color? colour = null)
        {
            UnityEditor.Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (colour.HasValue) GUI.color = colour.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
            {
                GUI.color = restoreColor;
                UnityEditor.Handles.EndGUI();
                return;
            }

            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
        }

        protected void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.grey;
                Gizmos.DrawWireCube(SumoVehicle.Position, Vector3.one * 2);
                DrawString(SumoVehicle.Speed.ToString(), SumoVehicle.Position, Color.black);
                Gizmos.DrawLine(SumoVehicle.Position, transform.position);
                //DrawString(m_carController.MaxSpeed.ToString(), m_waypointProgressTracker.TargetData.position);
            }
        }
#endif
    }

}
