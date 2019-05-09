using System.Collections;
using System.Collections.Generic;
using RiseProject.Kirlitsias;
using UnityEngine;
using UnityQuery;

namespace UtilityAI
{
    public enum State { GOAL_FOLLOWING,TOWARDS_CROSSING, WANTS_TO_CROSS, CROSSING };

    /// <summary>
    /// Cache my goals.I will probably build more logic in this one.
    /// This should be used when we want to keep an original goal that there is a chance of temporarily changing to
    /// other intermediate goals.For example other points of interests that came up during agents travel, or a crossing etc.
    /// </summary>
    public class GoalCache
    {
        private List<Vector3> cache = new List<Vector3>();
        private Vector3 temporaryV3;

        /// <summary>
        /// Should be used when you are not following the original goal, because it removes the current goal.
        /// IN the case of the original goal we dont want to remove it as we need to follow it in the end.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNextGoal()
        {
            if (cache.Count > 0)
            {
                cache.RemoveAt(0);
                return cache[0];
            }
            Debug.LogError("NO goals available");
            return Vector3.zero;
        }
        /// <summary>
        /// This function returns the current goal without removing it.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCurrentGoal()
        {
            if(cache.Count > 0)
            {
                return cache[0];
            }
            Debug.LogError("NO goals available");
            return Vector3.zero;
        }
        /// <summary>
        /// Shifts the first placed goal, usually because immediate goals were found, due to crossing needs.
        /// </summary>
        public void ShiftFirstToLast()
        {
            cache.Add(cache[0]);
            cache.RemoveAt(0);
        }
        /// <summary>
        /// Adds positions that are to be reached before the current goal.Can be used for other interests that came up 
        /// before reaching the original goal.
        /// </summary>
        public void AddIntermediateGoal(Vector3 toAdd)
        {
            if (cache.Count == 0)
            {
                Debug.LogError("This should not be empty");
                return;
            }
            cache.Add(toAdd);
        }

        public void AddIntermediateGoalFirst(Vector3 toAdd)
        {
            if (cache.Count == 0)
            {

                Debug.LogError("This should not be empty");
                return;
            }
            cache.Insert(0, toAdd);
        }

        /// <summary>
        /// Returns the point across the road when we talk about a crossing.
        /// Should not be called in any other occasions!
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Vector3 GetPointAcrossTheRoad(State state)
        {
            if (state != State.WANTS_TO_CROSS)
            {
                Debug.LogError("Should not call me.I am a special function for a certain occasion:)");
                return Vector3.zero;
            }
            if (cache.Count < 2)
            {
                Debug.LogError("There must be something wrong with the programs logic");
            }
            return cache[1];
        }

        /// <summary>
        /// Modifies the point across the road.THis should be used when the agent moves parallel to
        /// the sidewalk waiting for clean road.
        /// Should not be called in any other occasions!
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public void ModifyPointAcrossTheRoad(State state,Vector3 modifyBy)
        {
            if (state != State.WANTS_TO_CROSS)
            {
                Debug.LogError("Should not call me.I am a special function for a certain occasion:)");
                return;
            }
            if (cache.Count < 2)
            {
                Debug.LogError("There must be something wrong with the programs logic");
                return;
            }
            for (int i = 1; i < cache.Count-1; i++)
            {
                cache[i] += modifyBy;
            }
            
            //return cache[1];
        }


        //public 
        /// <summary>
        /// RE-Initializes the cache by clearing the underlying list and then adding the current goal
        /// that was made available from another aspect of the simulation.
        /// </summary>
        /// <param name="newGoalAcquired"></param>
        public void ResetCache(Vector3 newGoalAcquired)
        {
            cache.Clear();
            cache.Add(newGoalAcquired);
        }

        //5+1 - 6=0 -> 6-5 =1.... Should work. Mpakalies koumpare.
        public int GetIndexOfCurrentIntermediateGoal(List<CrossingPoint> crossingPoints)
        {
            return crossingPoints.Count+1 - cache.Count;
        }
    }

    public class Agent : MonoBehaviour, IContextProvider
    {
        public Brain wantsToCrossBrain; // Assign this in the editor; One Brain is a "type" of agent, so shared by multiple agents
        public Brain crossingBrain;
            
        AI ai; // Instance connecting the AI character with its Brain
        HumanContext context;
        private State state;
        private bool goalInitialized = false;
        private int currentGoal = 0;
        public IAgentType agentType;
        private Vector3 crossingStartingPoint, crossingEndPoint;
        //private Transform sidewalkWantsToCross,oppositeSidewalk = null;
        private GoalCache cache=new GoalCache();
        private GoalManager goalManager;
        /// <summary>
        /// Should use unity units.
        /// Also its important to change this in the future and find a nice interface to use 
        /// in order to achieve better realism lul. Should probably be member of a character profile that 
        /// in the future will be used to fit various real people characters.
        /// </summary>
        private float maximumSpeed = 7;
        /// <summary>
        /// We take into consideration unity units. Should change as well.
        /// </summary>
        private float comfortSpeed = 1.55f;

        public State State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
            }
        }

        public GoalCache Cache
        {
            get
            {
                return cache;
            }

            set
            {
                cache = value;
            }
        }

        public float MaximumSpeed
        {
            get
            {
                return maximumSpeed;
            }

            set
            {
                maximumSpeed = value;
            }
        }

        public float ComfortSpeed
        {
            get
            {
                return comfortSpeed;
            }

            set
            {
                comfortSpeed = value;
            }
        }

        public IntersectionDetails Details
        {
            get
            {
                return details;
            }

            set
            {
                details = value;
            }
        }

   

        public IContext GetContext()
        {
            return context;
        }

        // Use this for initialization
        void Start()
        {
            context = new HumanContext()
            {
                agent = this
            };
            agentType = new AgentTypeNavmesh(this, context);
            ai = new AI(wantsToCrossBrain);
            goalManager = GetComponent<GoalManager>();
#if UNITY_EDITOR
            // Add hook so the debugger show the agents state when you select it
            var debuggerHook = context.agent.gameObject.AddComponent<AIDebuggingHook>();
            debuggerHook.ai = ai;
            debuggerHook.contextProvider = this;
#endif
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //details
            UpdateContext();
            //if(details!=null)
            //    Debug.DrawRay(details.intersection, details.GetPerpendicular().normalized.YTOZ() * 5, Color.green);

            //ai.Process(this);
        }
        void UpdateContext()
        {
            // In real code you would scan for enemies here (Physics.OverlapSphere) or compute cover positions via raycasts 
            CheckForInitialization();
            StateMachine();
        }
        private void StateMachine(){
            switch (State)
            {
                case State.GOAL_FOLLOWING:
                    GoalFollowing();
                    break;
                case State.TOWARDS_CROSSING:
                    TowardsCrossing();
                    break;
                case State.CROSSING:
                    Crossing();
                    break;
                case State.WANTS_TO_CROSS:
                    WantsToCross();
                    break;
                default:
                    break;
            }
        }
        private void GoalFollowing()
        {
            ///Look whether current goal intersects a crossing.If yes temporarily change goal towards crossing and when reaching the goal,
            ///change state to wants to cross.
            ///NearGoal? change goal
            if (DoesMyPathCrossTheRoad())
            {
                ///This works because i already changed the order of the goals in cache.
                SetStateTowardsCrossing();
            }
            else
            {
                
                agentType.MoveTowardsGoal();
                ///Should add some logic for reaching the goal.

                if (agentType.CheckDistanceToGoal()){
                    OriginalGoalReached();
                }
            }
        }
        private void OriginalGoalReached()
        {
            //Debug.Log("Reached goal");
            context.originalGoal = goalManager.GetNextGoal().position;
            Cache.ResetCache(context.originalGoal);
            context.goal = context.originalGoal;
            agentType.SetGoalPosition(context.goal);
            details = new IntersectionDetails();
            context.details = details;
        }

        private void TowardsCrossing()
        {
            ///The agent is approaching the sidewalk.In this state the agent does not have to 
            ///look for any crossings. THis was the main reason to create a new State. Should we enable the agent to go back?
            ///Maybe an overriding function, that works on its own throughout the agents mental state. The thing that decides
            ///the original goals should be able to reset completely these states.
            agentType.MoveTowardsGoal();
            if (agentType.CheckDistanceToGoal())
            {
                SetStateWantsToCross();
            }
        }
        private void WantsToCross()
        {
            ///TRigger Utility ai that decides whether the agent should move parallel
            ///(On the sidewalk still waiting the right time to cross) or 
            ///Start crossing.If the agent starts to cross he changes state to Crossing.
            ///How to implement?
            ///The action should be able to manipulate the goal and the state. During this state we can
            ///make the agent move across the sidewalk, if it is logical to do(logical means that the agent will cover some
            ///distance while moving across the sidewalk) waiting for an opportunity.
            ai.Process(this);

        }
        private void Crossing()
        {
            ///While the agent is crossing, the agent should still look for the cars in the area.
            ///If for any reason the danger felt from the environment increases the agent should try find the best 
            ///way out for him. Depending on the danger we should allow the agent to move freely in the road.
            ai.Process(this);
            ///This will need fix later, when the agent will be able to move towards the sidewalk.
            if (agentType.CheckDistanceToGoal())
            {
                //Debug.Log("Reached");
                if (details.crossingPoints[cache.GetIndexOfCurrentIntermediateGoal(details.crossingPoints)].Type ==
                    CrossingPoint.CrossingPointType.END)
                {                   
                    SetStateGoalFollowing();
                }
                else
                {
                    SetStateWantsToCross();
                }
            }
        }
        
        public void SetStateWantsToCross()
        {
            State = State.WANTS_TO_CROSS;
            ai = new AI(wantsToCrossBrain);
        }
        public void SetStateGoalFollowing()
        {
            context.goal = cache.GetNextGoal();
            agentType.SetGoalPosition(context.goal);
            State = State.GOAL_FOLLOWING;
        }
        public void SetStateCrossing()
        {
            State = State.CROSSING;
            ai = new AI(crossingBrain);
            context.goal = Cache.GetNextGoal();
            agentType.SetGoalPosition(context.goal);
        }
        public void SetStateTowardsCrossing()
        {
            context.goal = cache.GetCurrentGoal();
            agentType.SetGoalPosition(context.goal);
            State = State.TOWARDS_CROSSING;
        }

        void CheckForInitialization()
        {
            //if (context.cars == null)
            //    context.cars = GameObject.FindGameObjectsWithTag("car");
            if (context.graphRoadNetwork == null)
            {
                context.graphRoadNetwork = GameObject.Find("Graph").GetComponent<SimpleGraphRoadNetwork>();
            }
            //if (context.sidewalks == null)
            //    context.sidewalks = GameObject.FindGameObjectsWithTag("sidewalk");
            //if (context.crossings == null)
            //    context.crossings = GameObject.FindGameObjectsWithTag("crossing");
            if (!goalInitialized)
            {
                goalInitialized = true;
                context.originalGoal = goalManager.GetNextGoal().position;
                context.goal = context.originalGoal;
                cache.ResetCache(context.goal);

            }
            //agentType.SetGoalPosition(context.goal);


        }

        private void UpdateGoal()
        {

        }


        private float distanceToSafety; IntersectionDetails details = null;
        /// <summary>
        /// Exceprt from this approach, we could look for two intersections from any crossing, not just pairs.
        /// 
        /// </summary>
        /// <returns></returns>
        private bool DoesMyPathCrossTheRoad()
        {
            Vector3 temp;SimpleGraphRoadNetwork.Edge edge;
            
            if(context.graphRoadNetwork.FindIntersectionOfPathWithRoads(transform,out temp,out edge,out details))
            {
                if (details.sideWalkDirection!=Vector3.zero && Intersection.CheckIfLeft(transform, details.GetPoint1(), details.GetPoint2()))
                {
                    //Debug.Log("Wrong direction");
                    return false;
                }
                //Debug.Log(details);
                //Debug.Log("Found with new algo");
                //edge.FindNearestPoint(transform.position, out temp);
                //new GameObject().transform.position = details.intersection;
                Debug.DrawRay(details.intersection, details.GetPerpendicular().normalized * 5, Color.green);
                
                cache.ShiftFirstToLast();
                context.details = details;
                details.crossingPoints.Clear();
                details.FindInBetweenNodes();
                details.mAgent = this;
                foreach(var item in details.crossingPoints)
                {
                    cache.AddIntermediateGoal(item.SafetyPosition);
                    //new GameObject().transform.position = item.SafetyPosition;
                }
                cache.ShiftFirstToLast();
                return true;
            }
            return false;
            Vector3 intersectionPoint,tmp,nearestIntersectionPoint=Vector3.zero;
            Transform crossingIntersected=null;
            float distanceOfIntersection=float.MaxValue;
            int indexOfNearest = 0;
            for (int i=0;i < context.crossings.Length; i += 1)
            {
                if (!CheckIfInCrossing(context.crossings[i].transform))
                    continue;

                for(int j = 0; j < 2; j++)
                {
                    Debug.DrawRay(transform.position, context.goal - transform.position, Color.red);
                    Debug.DrawRay(context.crossings[i].transform.GetChild(j).GetChild(0).position,
                        context.crossings[i].transform.GetChild(j).GetChild(1).position
                        - context.crossings[i].transform.GetChild(j).GetChild(0).position, Color.yellow);

                    if (Intersection.LineSegmentsIntersection(transform.position, context.goal,
                    context.crossings[i].transform.GetChild(j).GetChild(0).position, context.crossings[i].transform.GetChild(j).GetChild(1).position, out intersectionPoint))
                    {
                        crossingIntersected = context.crossings[i].transform;
                        if (distanceOfIntersection > (intersectionPoint - transform.position).magnitude)
                        {

                            distanceOfIntersection = (intersectionPoint - transform.position).magnitude;
                            nearestIntersectionPoint = intersectionPoint;
                            indexOfNearest = j;
                        }
                    }
                }
                if (crossingIntersected != null)
                    break;
            }
            if (crossingIntersected == null)
            {
                return false;
            }
            tmp = nearestIntersectionPoint;
            cache.AddIntermediateGoal(nearestIntersectionPoint);
            //new GameObject().transform.position = nearestIntersectionPoint;
            Vector3 o = crossingIntersected.GetChild(indexOfNearest).GetChild(1).position - crossingIntersected.GetChild(indexOfNearest).GetChild(0).position;
            o = Vector2.Perpendicular(o.XZ()).YTOZ();
            Vector3 other = crossingIntersected.GetChild((indexOfNearest+1)%2).GetChild(1).position - crossingIntersected.GetChild((indexOfNearest + 1) % 2).GetChild(0).position;
            Debug.DrawRay(nearestIntersectionPoint, o, Color.red);
            Debug.DrawRay(crossingIntersected.GetChild((indexOfNearest + 1) % 2).GetChild(0).position, other, Color.green);
            if (Intersection.LineSegmentsIntersection(nearestIntersectionPoint, o,
             crossingIntersected.GetChild((indexOfNearest + 1) % 2).GetChild(0).position, other, out intersectionPoint))
            {
                distanceToSafety = (intersectionPoint - tmp).magnitude;//This line here calculates the distance to safety for the crossing found.
                cache.AddIntermediateGoal(intersectionPoint);
                //new GameObject().transform.position = intersectionPoint;
            }
            //sidewalkWantsToCross = crossingIntersected.GetChild(indexOfNearest);
            //oppositeSidewalk= crossingIntersected.GetChild((indexOfNearest + 1) % 2);
            cache.ShiftFirstToLast();
            return true;
        }


        /// <summary>
        /// Should provide the time that the agents need to  reach safety.
        /// Depending on the agents' current state,different way of calculating this value is used.
        /// </summary>
        /// <returns></returns>
        public float TimeToSafetyComfort()
        {
            switch (State)
            {
                case State.CROSSING:
                    Debug.Log("DOes nothing for now");//Should be fixed soon!
                    break;
                case State.WANTS_TO_CROSS:
                    return distanceToSafety / comfortSpeed;
                default:
                    Debug.LogError("Should not be here re gare");
                    break;
            }
            Debug.LogError("Wrong time to safety returned");
            return float.PositiveInfinity;
        }
        /// <summary>
        /// Provides the time that the agents need to reach safety being in a hurry though.
        /// Depending on the agents' current state,different way of calculating this value is used.
        /// </summary>
        /// <returns>A time value, exei provlima?</returns>
        public float TimeToSafetyUrgent()
        {
            switch (State)
            {
                case State.CROSSING:
                    Debug.Log("DOes nothing for now");//Should be fixed soon!
                    break;
                case State.WANTS_TO_CROSS:
                    return distanceToSafety / maximumSpeed;
                default:
                    Debug.LogError("Should not be here re gare");
                    break;
            }
            Debug.LogError("Wrong time to safety returned");
            return float.PositiveInfinity;
        }
        private bool CheckIfInCrossing(Transform crossing)
        {
            Vector3 B,A;
            B = crossing.GetChild(0).GetChild(0).position;
            A = crossing.GetChild(0).GetChild(1).position;

            float position1 = Mathf.Sign((B.x - A.x) * (transform.position.z - A.z) - (B.z - A.z) * (transform.position.x - A.x));

            B = crossing.GetChild(1).GetChild(0).position;
            A = crossing.GetChild(1).GetChild(1).position;

            float position2 = Mathf.Sign((B.x - A.x) * (transform.position.z - A.z) - (B.z - A.z) * (transform.position.x - A.x)); ;

            if(position1>0 && position2 < 0)
            {
                return false;
            }
            if (position1 < 0 && position2 > 0)
            {
                return false;
            }
            return true;
        }

        //public Vector3 GetCurrentSidewalkDirection()
        //{
        //   return (sidewalkWantsToCross.GetChild(1).position - sidewalkWantsToCross.GetChild(0).position).normalized.normalized;
        //}
        /// <summary>
        /// Call this function only whlie the pedestrian is on WANTS_TO_CROSS state.
        /// It returns vector.zero if not at correct state.
        /// </summary>
        /// <returns></returns>
        public Vector3 AgentPreferredVelocityOnCrossing()
        {
            if (state != State.WANTS_TO_CROSS)
            {
                Debug.LogError("Should not call me because i am not in the correct state.");
                return Vector3.zero;
            }
            Vector3 originalGoalDirection=(context.originalGoal-transform.position).normalized;
            Vector3 crossingGoalDirection = ((cache.GetPointAcrossTheRoad(state))-transform.position).normalized;
            return (originalGoalDirection + crossingGoalDirection).normalized * ((AgentTypeNavmesh)agentType).nav.speed;
        }
    }
}