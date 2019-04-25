using UnityEngine;


namespace RiseProject.Tomis.VehicleControl
{
    [RequireComponent(typeof(CarVisualController))]
    public class VehicleMoverByTransform : VehicleMover
    {
        /// <summary> Raycasting for height calculation </summary>
        protected CarRayCaster CarRayCaster { get; private set; }
        protected RaycastHit _frontHit;
        protected RaycastHit _backHit;

        protected override void Awake()
        {
            base.Awake();
            
            VisualController.HandleWheelRotations = true;

            CarRayCaster = GetComponent<CarRayCaster>();
        }

        /// <summary>
        /// Change Car Angle for turns and slopes.
        /// </summary>
        protected virtual void ChangeCarAngle()
        {
            return;
/*
            Vector3 frontHitPos, backHitPos;
            Vector3 heading;
            Vector3 directionA;
            if (CarRayCaster.FrontRayDidHit && CarRayCaster.BackRayDidHit)
            {
                frontHitPos = _frontHit.point;
                backHitPos = _backHit.point;
                heading = (frontHitPos - backHitPos);
                directionA = heading / heading.magnitude;
                bool isPitchAngleNegative = frontHitPos.y > backHitPos.y;

                backHitPos.y = frontHitPos.y;

                heading = (frontHitPos - backHitPos);
                Vector3 directionB = heading / heading.magnitude;

                float pitchAngle = isPitchAngleNegative ? -Vector3.Angle(directionA, directionB) : Vector3.Angle(directionA, directionB);

                //Debug.Log("FrontHitPos y " + frontHitPos.y + "BackHitPosY" + backHitPos.y);
                if (frontHitPos.y > backHitPos.y)
                    pitchAngle = -pitchAngle;

                transform.localRotation = Quaternion.Euler(new Vector3(pitchAngle, SumoVehicle.Angle, 0f));
                Debug.DrawLine(new Vector3(0, 0, 0), directionA, UnityEngine.Color.green);
                Debug.DrawLine(new Vector3(0, 0, 0), directionB, UnityEngine.Color.green);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0f, SumoVehicle.Angle, 0f);
            }
*/
        }
    }
}