using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class WaypointProgressTracker : MonoBehaviour
    {
        // This script can be used with any object that is supposed to follow a
        // route marked out by waypoints.

        // This script manages the amount to look ahead along the route,
        // and keeps track of progress and laps.

        [SerializeField] private WaypointCircuit circuit; // A reference to the waypoint-based route we should follow

        [SerializeField] private float lookAheadForTargetOffset = 5;
        // The offset ahead along the route that the we will aim for

        [SerializeField] private float lookAheadForTargetFactor = .1f;
        // A multiplier adding distance ahead along the route to aim for, based on current speed

        [SerializeField] private float lookAheadForSpeedOffset = 10;
        // The offset ahead only the route for speed adjustments (applied as the rotation of the waypoint target transform)

        [SerializeField] private float lookAheadForSpeedFactor = .2f;
        // A multiplier adding distance ahead along the route for speed adjustments

        [SerializeField] private ProgressStyle progressStyle = ProgressStyle.SmoothAlongRoute;
        // whether to update the position smoothly along the route (good for curved paths) or just when we reach each waypoint.

        [SerializeField] private float pointToPointThreshold = 4;
        // proximity to waypoint which must be reached to switch target to next waypoint : only used in PointToPoint mode.

        public enum ProgressStyle
        {
            SmoothAlongRoute,
            PointToPoint,
        }

        // these are public, readable by other objects - i.e. for an AI to know where to head!
        public WaypointCircuit.RoutePoint targetPoint { get; protected set; }
        public WaypointCircuit.RoutePoint speedPoint { get; protected set; }
        public WaypointCircuit.RoutePoint progressPoint { get; protected set; }

        protected float LookAheadForTargetOffset { get => lookAheadForTargetOffset; private set => lookAheadForTargetOffset = value; }
        protected float LookAheadForTargetFactor { get => lookAheadForTargetFactor; private set => lookAheadForTargetFactor = value; }
        protected float LookAheadForSpeedOffset { get => lookAheadForSpeedOffset; private set => lookAheadForSpeedOffset = value; }
        protected float LookAheadForSpeedFactor { get => lookAheadForSpeedFactor; private set => lookAheadForSpeedFactor = value; }
        protected ProgressStyle ProgStyle { get => progressStyle; set => progressStyle = value; }
        protected float ProgressDistance { get => progressDistance; set => progressDistance = value; }
        protected int ProgressNum { get => progressNum; set => progressNum = value; }
        protected Vector3 LastPosition { get => lastPosition; set => lastPosition = value; }
        protected float Speed { get => speed; set => speed = value; }
        protected float PointToPointThreshold { get => pointToPointThreshold; set => pointToPointThreshold = value; }
        protected WaypointCircuit Circuit { get => circuit; set => circuit = value; }

        public Transform target;

        private float progressDistance; // The progress round the route, used in smooth mode.
        private int progressNum; // the current waypoint number, used in point-to-point mode.
        private Vector3 lastPosition; // Used to calculate current speed (since we may not have a rigidbody component)
        private float speed; // current speed of this object (calculated from delta since last frame)

        // setup script properties
        protected virtual void Start()
        {
            // we use a transform to represent the point to aim for, and the point which
            // is considered for upcoming changes-of-speed. This allows this component
            // to communicate this information to the AI without requiring further dependencies.

            // You can manually create a transform and assign it to this component *and* the AI,
            // then this component will update it, and the AI can read it.
            if (target == null)
            {
                target = new GameObject(name + " Waypoint Target").transform;
            }

            Reset();
        }


        // reset the object to sensible values
        protected virtual void Reset()
        {
            ProgressDistance = 0;
            ProgressNum = 0;
            if (ProgStyle == ProgressStyle.PointToPoint)
            {
                target.position = Circuit.Waypoints[ProgressNum].position;
                target.rotation = Circuit.Waypoints[ProgressNum].rotation;
            }
        }


        protected virtual void Update()
        {
            if (ProgStyle == ProgressStyle.SmoothAlongRoute)
            {
                // determine the position we should currently be aiming for
                // (this is different to the current progress position, it is a a certain amount ahead along the route)
                // we use lerp as a simple way of smoothing out the speed over time.
                if (Time.deltaTime > 0)
                {
                    Speed = Mathf.Lerp(Speed, (LastPosition - transform.position).magnitude/Time.deltaTime,
                                       Time.deltaTime);
                }
                target.position =
                    Circuit.GetRoutePoint(ProgressDistance + LookAheadForTargetOffset + LookAheadForTargetFactor*Speed)
                           .position;
                target.rotation =
                    Quaternion.LookRotation(
                        Circuit.GetRoutePoint(ProgressDistance + LookAheadForSpeedOffset + LookAheadForSpeedFactor*Speed)
                               .direction);


                // get our current progress along the route
                progressPoint = Circuit.GetRoutePoint(ProgressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    ProgressDistance += progressDelta.magnitude*0.5f;
                }

                LastPosition = transform.position;
            }
            else
            {
                // point to point mode. Just increase the waypoint if we're close enough:

                Vector3 targetDelta = target.position - transform.position;
                if (targetDelta.magnitude < PointToPointThreshold)
                {
                    ProgressNum = (ProgressNum + 1)%Circuit.Waypoints.Length;
                }


                target.position = Circuit.Waypoints[ProgressNum].position;
                target.rotation = Circuit.Waypoints[ProgressNum].rotation;

                // get our current progress along the route
                progressPoint = Circuit.GetRoutePoint(ProgressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    ProgressDistance += progressDelta.magnitude;
                }
                LastPosition = transform.position;
            }
        }


        protected virtual void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.DrawWireSphere(Circuit.GetRoutePosition(ProgressDistance), 1);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(target.position, target.position + target.forward);
            }
        }
    }
}
