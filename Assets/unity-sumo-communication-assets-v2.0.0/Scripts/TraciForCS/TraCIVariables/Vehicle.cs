using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using RiseProject.Tomis.Util.TraciAuxilliary;
using RiseProject.Tomis.CustomTypes;
using System.Linq;
using CodingConnected.TraCI.NET;
using CodingConnected.TraCI.NET.Types;

namespace RiseProject.Tomis.SumoInUnity.SumoTypes
{
    [CreateAssetMenu(fileName = "Vehicle",menuName = @"TraciVariable\Vehicle"), Serializable]
    public class Vehicle : TraCIVariable
    {
        public static byte ContextDomain { get; } = TraCIConstants.CMD_GET_VEHICLE_VARIABLE;
        
        #region Vehicle States

        public enum SimulationState
        {
            None,
            Departed,
            Active, //TODO Set up vehicle to be active if no more just departed and is currently active in simulation
            Arrived   
        }
        
        public enum ContextSubscriptionState
        {
            None,
            JustEnteredContextRange,
            InsideContextRange,
            ExitedContextRange   
        }

        private ContextSubscriptionState _subscriptionState = ContextSubscriptionState.None;

        public ContextSubscriptionState SubscriptionState
        {
            get => _subscriptionState;
            set
            {
                _subscriptionState = value;
                switch (value)
                {
                    case ContextSubscriptionState.JustEnteredContextRange:
                        OnVehicleEnteredContextRange(new VehicleArgs(CreateTransform()));
                        break;
                    case ContextSubscriptionState.ExitedContextRange:
                        OnVehicleExitedContextRange(new VehicleArgs(CreateTransform()));
                        break;
                }
            }
        }

        
        private SimulationState _vehicleState = SimulationState.None;
        
        public SimulationState VehicleState
        {
            get => _vehicleState;
            set
            {
                _vehicleState = value;
                switch (value)
                {
                    case SimulationState.Departed:
                        OnVehicleDeparted(new VehicleArgs(CreateTransform()));
                        break;
                    case SimulationState.Arrived:
                        OnVehicleArrived(new VehicleArgs(CreateTransform()));
                        break;
                }
            }
        }

        #endregion
        
        #region Events
        
        public class VehicleArgs : EventArgs
        {
            /// <summary>
            /// The projected transform as it should be in unity.
            /// </summary>
            public readonly MyTransform VehicleTransform;
            public VehicleArgs(MyTransform t)
            {
                VehicleTransform = t;
            }
        }
        
        // lock for VehicleTransformChanged
        private readonly object _objectLock = new object();
        private event EventHandler<VehicleArgs> _vehicleTransformChanged;
        /// <summary>
        /// Raised whenever all positional and orientation data are changed.
        /// </summary>
        public event EventHandler<VehicleArgs> VehicleTransformChanged
        {
            add
            {
                lock (_objectLock)
                {
                    _vehicleTransformChanged += value;
                    createNewTransformEachStep = true;
                }
            }
            remove
            {
                lock (_objectLock)
                {
                    _vehicleTransformChanged -= value;
                    if(_vehicleTransformChanged == null) 
                    {
                        // If no subscribers left no need to make 
                        // new EventArgs each step
                        createNewTransformEachStep = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// Raised when the vehicle departs in the sumo simulation.
        /// </summary>
        public event EventHandler<VehicleArgs> VehicleDeparted;
        
        /// <summary>
        /// Raised when the vehicle arrives its destination in the sumo simulation.
        /// </summary>
        public event EventHandler<VehicleArgs> VehicleArrived;

        /// <summary>
        /// Raised when the vehicle enters Context Range.
        /// </summary>
        public event EventHandler<VehicleArgs> VehicleEnteredContextRange;
        
        /// <summary>
        /// Raised when the vehicle exits Context Range.
        /// </summary>
        public event EventHandler<VehicleArgs> VehicleExitedContextRange;
        
        protected virtual void OnVehicleTransformChanged(VehicleArgs e)
        {
            _vehicleTransformChanged?.Invoke(this, e);
        }
        
        
        protected virtual void OnVehicleDeparted(VehicleArgs e)
        {
            VehicleDeparted?.Invoke(this, e);
        }
        
        protected virtual void OnVehicleArrived(VehicleArgs e)
        {
            VehicleArrived?.Invoke(this, e);
        }
        
        
        protected virtual void OnVehicleEnteredContextRange(VehicleArgs e)
        {
            VehicleEnteredContextRange?.Invoke(this, e);
        }
        
        protected virtual void OnVehicleExitedContextRange(VehicleArgs e)
        {
            VehicleExitedContextRange?.Invoke(this, e);
        }
        
        #endregion

        public bool IsEligibleForDeletion { get; set; }

        [SerializeField, ReadOnly] private int stepsInSimulation = 0;
        [SerializeField, ReadOnly] private int numberOfPositionsToSave;
        [SerializeField, HideInInspector] private Vector2 raw2DPosition;
        [SerializeField, ReadOnly] private Vector3 position2DConverted;
        [SerializeField, ReadOnly] private Vector3 position3D;
        [SerializeField, ReadOnly] private Vector3 prevPosition2D;
        [SerializeField] private float angle;
        [SerializeField, ReadOnly] private float prevAngle;
        [SerializeField] private float speed;
        [SerializeField] private float acceleration;
        [SerializeField] private float width;
        [SerializeField] private float length;
        [SerializeField] private float height;
        [SerializeField] private int signal;
        [SerializeField, ReadOnly] private string routeId;
        [SerializeField, ReadOnly] private string edgeId;
        [SerializeField, ReadOnly] private string laneId;
        [SerializeField, ReadOnly] private int laneIndex;
        [SerializeField, ReadOnly] private float maxAcceleration;
        [SerializeField, ReadOnly] private List<string> edgeIDs;
        [SerializeField, ReadOnly] private float departStep;
        [SerializeField, ReadOnly] private LinkedList<MyTransform> positions = new LinkedList<MyTransform> ();

        private Transform _attachedVehicleTransform;

        public void SetPositionFromRawPosition2D(Position2D pos) => Raw2DPosition = TraCIAuxiliaryMethods.Raw2DPositionToVector2(pos);
        public void SetPositionFromVector2Position(Vector2 pos) => Raw2DPosition = pos;

        /// <summary> To update position and orientation only once both angle and position changed   </summary>
        private bool _isAngleChanged;
        /// <summary> To update position and orientation only once both angle and position changed </summary>
        private bool _isPositionChanged;


        /// <summary> The current Angle in degrees of this car. </summary>
        public float Angle
        {
            get => angle;
            set
            {
                PrevAngle = angle;
                angle = value;
                _isAngleChanged = true;

                /* Update Position and Orientation only when both position and angle changed */
                if (_isPositionChanged)
                {
                    UpdateVehiclePositionAndOrientation();
                    ResetChangeStage();
                }
            }
        }

        /// <summary> The raw position as it was received from the SumoClient. </summary>
        public Vector2 Raw2DPosition {
            get => raw2DPosition;
            private set
            {
                raw2DPosition = value;
                prevPosition2D = position2DConverted;
                position2DConverted = new Vector3(raw2DPosition.x, 0f, raw2DPosition.y);
                _isPositionChanged = true;

                /* Update Position and Orientation only when both position and angle changed */

                if (_isAngleChanged)
                {
                    UpdateVehiclePositionAndOrientation();
                    ResetChangeStage();
                }
                
            }
        }

        /// <summary>
        /// Sets <see cref="_isPositionChanged"/> and <see cref="_isAngleChanged"/>
        /// back to false
        /// </summary>
        private void ResetChangeStage()
        {
            _isAngleChanged = false;
            _isPositionChanged = false;
        }

        /// <summary>
        /// Updates position and orientation and emits event <see cref="OnVehicleTransformChange"/>
        /// with
        /// </summary>
        private void UpdateVehiclePositionAndOrientation()
        {
            VehicleArgs eventArgs = null;
            /* This is done to improve efficiency if no additional positions are needed.
             If no additional positions are needed two News are saved */
            if (createNewTransformEachStep)
            {
                var t = CreateTransform();
                eventArgs = new VehicleArgs(t);

                PositionAndOrientation.position = t.position;
                PositionAndOrientation.angle = t.angle;
                PositionAndOrientation.rotation = t.rotation;
            }
            else
            {
                PositionAndOrientation.position = Position;
                PositionAndOrientation.angle = Angle;
                PositionAndOrientation.rotation = Quaternion.Euler(new Vector3(0f, Angle, 0f));
            }
            OnVehicleTransformChanged(eventArgs);
        }

        private MyTransform CreateTransform()
        {
            var t = new MyTransform
            {
                position = Position,
                angle = Angle,
                rotation = Quaternion.Euler(new Vector3(0f, Angle, 0f))
            };

            if (NumberOfPositionsToSave > 0)
            {
                positions.RemoveFirst();
                positions.AddLast(t);
            }

            return t;
        }
        /// <summary> The position as converted for X,Z plane from the raw 2D position from sumo. (Y axis is 0f)  </summary>
        public Vector3 Position { get => position2DConverted; }
        /// <summary> The Position2D the vehicle had the last timestep as converted for X,Z plane from the raw 2D position from sumo. (Y axis is 0f)  </summary>
        public Vector3 PrevPosition2D { get => prevPosition2D; }
        /// <summary> The ids of the edges of this vehicle's route. </summary>
        public List<string> EdgeIDs { get => edgeIDs; set => edgeIDs = value; }
        public Vector3 Position3D { get => position3D; set => position3D = value; }
        /// <summary> The maximum acceleration this vehicle can achieve. </summary>
        public float MaxAcceleration { get => maxAcceleration; set => maxAcceleration = value; }
        /// <summary> The index of the lane this vehicle is on. </summary>
        public int LaneIndex { get => laneIndex; set => laneIndex = value; }
        /// <summary> The lane id this vehicle currently on. </summary>
        public string LaneId { get => laneId; set => laneId = value; }
        /// <summary> The edgeID this vehicle is currently on. </summary>
        public string EdgeId { get => edgeId; set => edgeId = value; }
        /// <summary>
        /// Route is an array of edges a vehicle is going to follow. The routeID is the 
        /// id of the route this vehicle has.
        /// </summary>
        public string RouteId { get => routeId; set => routeId = value; }
        /// <summary>
        /// Signal is an int intigating what the vehicle signals.
        /// e.g if the 2nd bit is set that means the car is blinking left.
        /// </summary>
        public int Signal { get => signal; set => signal = value; }
        /// <summary>
        /// Signal is an int intigating what the vehicle signals.
        /// e.g if the 2nd bit is set that means the car is blinking left.
        /// </summary>
        public float Height { get => height; set => height = value; }
        /// <summary> The length of this vehicle </summary>
        public float Length { get => length; set => length = value; }
        /// <summary> The width of this vehicle </summary>
        public float Width { get => width; set => width = value; }
        /// <summary> The previous (last simulation step) angle of this car. </summary>
        public float PrevAngle { get => prevAngle; set => prevAngle = value; }
        /// <summary> Speed of this vehicle in this step. </summary>
        public float Speed { get => speed; set => speed = value; }
        /// <summary> Vehicle's acceleration the last timestep </summary>
        public float Acceleration { get => acceleration; set => acceleration = value; }
        /// <summary> The Transform attached to this Vehicle when it departed </summary>
        public Transform AttachedVehicleTransform { get => _attachedVehicleTransform; set => _attachedVehicleTransform = value; }
        /// <summary> The step this vehicle departed </summary>
        public float DepartStep { get => departStep; set => departStep = value; }

        /// <summary> 
        /// Holds previous positions. Up to <see cref="NumberOfPositionsToSave"/> are cached. 
        /// The first [0] position is the oldest position and the last [<see cref="NumberOfPositionsToSave">] is the most recent. 
        /// </summary>
        public MyTransform[] Positions { get => positions.ToArray();}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[Vehicle Details --> ID: " + ID + " Position: " + Position.ToString("G4") + " Angle: " + Angle + " ]");
            return sb.ToString();
        }

        /* MOCK DATA FOR DEBUGGING PURPOSES */
        [SerializeField] private List<Vector3> mockNewPositions = new List<Vector3>();
        [SerializeField] private List<float> mockNewAngles = new List<float>();
        [SerializeField] private List<float> mockAccelerations = new List<float>();
        [SerializeField] private List<float> mockSpeeds = new List<float>();
        [SerializeField] private float mockStep;
        
        private int _selectedIndex;
        private Coroutine _runningCoroutine;

        public bool createNewTransformEachStep = true;


        public List<float> MockAngles { get => mockNewAngles; set => mockNewAngles = value; }
        public List<Vector3> MockPositions { get => mockNewPositions; set => mockNewPositions = value; }
        public float MockStep { get => mockStep; set => mockStep = value; }

        /// <summary>
        /// Instantiate Vehicle.
        /// </summary>
        /// <param name="id"> The id of this vehicle </param>
        /// <param name="numberOfPositionsToSave"> The number of positions that should be cached. Retrieved by <see cref="Positions"/> </param>
        public void Instantiate(string id, int numberOfPositionsToSave)
        {
            this.Instantiate(id);
            NumberOfPositionsToSave = numberOfPositionsToSave;
        }

        /// <summary>
        /// Instantiate Vehicle.
        /// </summary>
        /// <param name="numberOfPositionsToSave"> The number of Vehicles that should be saved </param>
        public void Instantiate(int numberOfPositionsToSave)
        {
            this.Instantiate("NA");
            NumberOfPositionsToSave = numberOfPositionsToSave;
        }

        /****************************                 VVV         EDITOR RELATED     VVV                     *******************************/
        /// <summary> Used by VehicleEditor for selecting a position (for deleting). DO NOT PLACE THIS IN EDITOR
        /// https://answers.unity.com/questions/483479/onscenegui-has-odd-behaviour-with-multiple-objects.html </summary>
        public int SelectedIndex { get => _selectedIndex; set => _selectedIndex = value; }
        public Coroutine RunningCoroutine { get => _runningCoroutine; set => _runningCoroutine = value; }
        public List<float> MockAccelerations { get => mockAccelerations; set => mockAccelerations = value; }
        public List<float> MockSpeeds { get => mockSpeeds; set => mockSpeeds = value; }

        /// <summary>
        /// The ammount of positions to cache for this Vehicle.
        /// </summary>
        public int NumberOfPositionsToSave
        {
            get => numberOfPositionsToSave;
            
            set
            {
                /* Create an array of fixed size. Positions will contain up to _numberOfPositionsToSave
                * with the last one being the most recent and the first the oldest */
                if (numberOfPositionsToSave != value)
                {
                    numberOfPositionsToSave = value;
                    positions.Clear();
                    MyTransform a = new MyTransform();
                    for(int i = 0; i < numberOfPositionsToSave; i++)
                    {
                        positions.AddLast(a);
                    }

                }

                createNewTransformEachStep = numberOfPositionsToSave > 0;

            }
        }

        /// <summary>
        /// The amount of steps in the simulation with departure being the first.
        /// Managed by <see cref="SumoClient"/>.
        /// </summary>
        public int StepsInSimulation { get => stepsInSimulation; set => stepsInSimulation = value; }
        public MyTransform PositionAndOrientation { get; } = new MyTransform();

        public IEnumerator ApplyMockData()
        {
        
            for (int i = 0; i < MockPositions.Count; i++)
            {
                Raw2DPosition = TraCIAuxiliaryMethods.Vector3toVector2(MockPositions[i]);
                Angle = MockAngles[i];
                yield return new WaitForSeconds(MockStep);
            }
        }

        public void ClearMockData()
        {
            MockAngles.Clear();
            MockPositions.Clear();
            MockAccelerations.Clear();
            MockSpeeds.Clear();
        }

    }
}