using RiseProject.Tomis.DataHolders;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;

namespace RiseProject.Tomis.VehicleControl
{

    [RequireComponent(typeof(Rigidbody), typeof(VehicleConfigurationData))]
    public class VehicleMoverByPhysics : VehicleMover
    {

        private Rigidbody m_vehicleRigidBody;
        private Transform m_vehicleTransform;

        [SerializeField] private WheelCollider _frontRightWheel, _frontLeftWheel;
        [SerializeField] private WheelCollider _rearRightWheel, _rearLeftWheel;
        [SerializeField] private Transform _frontRightTransform, _frontLeftTransform;
        [SerializeField] private Transform _rearRightTransform, _rearLeftTransform;

        [SerializeField] private float _maxSteerAngle = 30f;
        [SerializeField] private float _acceleration = 0f;
        private float _prevAngle;
        private float _vehicleTargetAngle;

        [SerializeField] private float motorTorque = 50f;

        private float _steerAngle = 0f;

        private float _wheelRadius;
        private float _vehicleMass;

        protected virtual WheelCollider FrontRightWheel { get => _frontRightWheel; set => _frontRightWheel = value; }
        protected virtual WheelCollider FrontLeftWheel { get => _frontLeftWheel; set => _frontLeftWheel = value; }
        protected virtual WheelCollider RearRightWheel { get => _rearRightWheel; set => _rearRightWheel = value; }
        protected virtual WheelCollider RearLeftWheel { get => _rearLeftWheel; set => _rearLeftWheel = value; }
        protected virtual Transform FrontRightTransform { get => FrontRightTransform1; set => FrontRightTransform1 = value; }
        protected virtual Transform FrontLeftTransform { get => _frontLeftTransform; set => _frontLeftTransform = value; }
        protected virtual Transform FrontRightTransform1 { get => _frontRightTransform; set => _frontRightTransform = value; }
        protected virtual Transform RearRightTransform { get => _rearRightTransform; set => _rearRightTransform = value; }
        protected virtual Transform RearLeftTransform { get => _rearLeftTransform; set => _rearLeftTransform = value; }
        protected virtual Rigidbody VehicleRigidBody { get => m_vehicleRigidBody; set => m_vehicleRigidBody = value; }
        protected virtual Transform VehicleTransform { get => m_vehicleTransform; set => m_vehicleTransform = value; }
        public virtual float WheelRadius { get => _wheelRadius; protected set => _wheelRadius = value; }
        public virtual float VehicleMass { get => _vehicleMass; protected set => _vehicleMass = value; }
        public virtual float Acceleration { get => _acceleration; protected set => _acceleration = value; }
        public virtual float PrevAngle { get => _prevAngle; protected set => _prevAngle = value; }
        public virtual float VehicleTargetAngle { get => _vehicleTargetAngle; protected set => _vehicleTargetAngle = value; }
        public virtual float SteerAngle { get => _steerAngle; protected set => _steerAngle = value; }
        public float MotorTorque { get => motorTorque; protected set => motorTorque = value; }
        public float MaxSteerAngle { get => _maxSteerAngle; private set => _maxSteerAngle = value; }

        protected override void OnEnable()
        {
            base.OnEnable();

            VehicleRigidBody = GetComponent<Rigidbody>();

            WheelRadius = FrontRightWheel.radius;
            VehicleMass = VehicleRigidBody.mass;
            VehicleTransform = VehicleRigidBody.transform;

            PrevAngle = SumoVehicle.Angle;
            VehicleTargetAngle = SumoVehicle.Angle;
        }

        protected virtual void CalculateTorque()
        {
            float speed = SumoVehicle.Speed;
            Acceleration = SumoVehicle.Acceleration;
            float force = (VehicleMass) * Acceleration;
            MotorTorque = force * WheelRadius;
        }


        protected virtual void CalculateSteer()
        {
            float transformYAngle = VehicleTransform.eulerAngles.y;
            if (Mathf.Approximately(transformYAngle, VehicleTargetAngle))
            {
                SteerAngle = 0f;

            }
            else
            {
                SteerAngle = VehicleTargetAngle - transformYAngle;

            }
        }

        protected virtual void ApplySteer()
        {
            FrontLeftWheel.steerAngle = SteerAngle;
            FrontRightWheel.steerAngle = SteerAngle;
        }

        protected virtual void Update()
        {
            // targetOrientation = Vehicle.Angle;
            PrevAngle = VehicleTargetAngle;
            VehicleTargetAngle = SumoVehicle.Angle;
        }

        protected virtual void Accelerate()
        {
            FrontRightWheel.motorTorque = MotorTorque;
            FrontLeftWheel.motorTorque = MotorTorque;
        }

        private void UpdateWheelPoses()
        {
            UpdateWheelPose(FrontRightWheel, FrontRightTransform);
            UpdateWheelPose(FrontLeftWheel, FrontLeftTransform);
            UpdateWheelPose(RearRightWheel, RearRightTransform);
            UpdateWheelPose(RearLeftWheel, RearLeftTransform);
        }

        private void UpdateWheelPose(WheelCollider wheelCollider, Transform wheelTransform)
        {
            Vector3 pos;// = wheelTransform.position;
            Quaternion quat;// = wheelTransform.rotation;

            wheelCollider.GetWorldPose(out pos, out quat);
            wheelTransform.transform.position = pos;
            wheelTransform.transform.rotation = quat;
        }

        private void Move()
        {
            VehicleRigidBody.MovePosition(SumoVehicle.Position3D);
        }

        protected virtual void FixedUpdate()
        {
            CalculateTorque();
            CalculateSteer();
            ApplySteer();
            Accelerate();
            UpdateWheelPoses();
        }

        protected override void Awake()
        {
            throw new System.NotImplementedException();
        }

        protected override void Vehicle_VehicleTransformChanged(object sender, Vehicle.VehicleArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}