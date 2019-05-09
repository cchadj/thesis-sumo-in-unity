using UnityEngine;

namespace RiseProject.Tomis.VehicleControl
{
    public class MoveByPhysicsFollowPath : VehicleMoverByPhysics
    {

        private Vector3 curTarget;
        [SerializeField]
        private float _changeTargetThreshold;
        protected override void OnEnable()
        {
            base.OnEnable();
            curTarget = SumoVehicle.Position;
        }

        protected override void Update()
        {
            if (Vector3.Distance(transform.position, curTarget) <= _changeTargetThreshold)
            {
                curTarget = SumoVehicle.Position;
            }
        }

        protected override void CalculateSteer()
        {
            Vector3 relativeVector = transform.InverseTransformDirection(curTarget);
            relativeVector.Normalize();

            SteerAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteerAngle;

        }

        protected override void CalculateTorque()
        {
            MotorTorque = MotorTorque;
        }


        public float ChangeTargetThreshold { get => _changeTargetThreshold; private set => _changeTargetThreshold = value; }
    }
}
