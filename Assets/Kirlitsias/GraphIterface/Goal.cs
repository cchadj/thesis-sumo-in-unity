using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RiseProject.Kirlitsias;
using UnityEngine;


public class Goal{

    protected Vector3 position;
    public Goal next;
    public Goal previous;
    public bool end = false;
    private float threshold = 0.25f;
    public Goal(Vector3 goal)
    {
        position = goal;
        next = null;
        previous = null;
    }

    public Goal()
    {
    }
    public virtual bool ReachedGoal(Vector3 agentPosition)
    {
        agentPosition.y = position.y;
        //Debug.Log("Distance is :" + (position - agentPosition).magnitude);
        if ((position - agentPosition).magnitude < threshold)
        {
            return true;
        }
        return false;
    }
    public virtual Vector3 GetFinalGoal()
    {
        Goal tmp = this;
        while (tmp.next != null&&!tmp.end)
            tmp = tmp.next;
        
        return tmp.position;
    }
    public virtual Vector3 GetGoalPosition()
    {
        return position;
    }
}

public class GoalController:IAgentOnRoadInfo
{

    private Goal currentGoal;
    private Goal endGoal;
    private IEntity info;
    public GameObject visualizeGoal;
    private IGoalNeeds goalNeeds;
    private CrossingStateMachine stateMachine;
    private StateLogic logic;
    private TimeToCollision times = new TimeToCollision();
    private CrossingFunction function = new CrossingFunction();

    public void SetProfileOfFunction(Profile prof)
    {
        function.MProfile = prof;
    }

    private void InitializeActivationLogic()
    {
        ActivationBasedStaeLogic logic = new ActivationBasedStaeLogic();

        this.logic = logic;
    }
    private void InitializeTimerLogic()
    {
        TimerBasedStateLogic logic = new TimerBasedStateLogic();
        logic.MoveOnSidewalk = MoveOnSidewalk;
        logic.ReachedEditableGoal = ReachedEditableGoal;
        logic.TransToFastForward = TransToFastForward;
        logic.TransToForward = TransToForward;
        logic.TransToWantsFreely = TransToWantsFreely;
        logic.TransToWantsToCross = TransToWantsToCross;
        logic.TransToBackward = TransToBackward;
        logic.times = times;
        this.logic = logic;
    }

    public CrossingStateMachine StateMachine
    {
        get
        {
            return stateMachine;
        }
    }

    public Vector3 EditableGoal
    {
        get
        {
            return editableGoal;
        }

        set
        {
            value.y = info.GetPosition().y;
            goalNeeds.SetGoal(value);
            editableGoal = value;
        }
    }

    public GoalController(IEntity info,IGoalNeeds goalNeeds)
    {
        this.info=info;
        this.goalNeeds = goalNeeds;
        InitializeTimerLogic();
    }

    private Vector3 referencePoint;
    /// <summary>
    /// Sets the reference point which is taken into consideration when looking for moving freely decisions etc.
    /// </summary>
    public void SetReferencePoint(Vector3 refe)
    {
        referencePoint = refe;
    }

    public int Step()
    {
   
        int val = 0;
        if ((val = CheckGoalReached())!=0)
        {
            AgentTimers.ResetAgentTimer(info,true);
            return val;
            state = WANTSTOCROSS;
            Debug.Log("Reachd END OF CROSSING");
            //return true;
            /// Probably dismiss? or this should be done in 
            /// another.
            /// 
        }
        if (currentGoal.previous is CrossingGoal)
        {
            ModifyGoal();
        }
        if (visualizeGoal != null)
            visualizeGoal.transform.position = currentGoal.GetGoalPosition();
        AgentTimers.ResetAgentTimer(info, true);
        return 0;
        //if (stateMachine != null)
        //    stateMachine.RunState();
    }

    private const int WANTSTOCROSS = 0;
    private const int FORWARD = 1;
    private const int BACKWARD = 2;
    private const int FASTFORWARD = 3;
    private const int FREELY = 4;
    private Vector3 editableGoal;
    private int state = WANTSTOCROSS;
    private void ModifyGoal()
    {
        times.UpdateTime((CrossingGoal)currentGoal, info);
        switch (state)
        {
            case WANTSTOCROSS:
                logic.WantsToCross();
                break;
            case FORWARD:
                logic.Forward();
                break;
            case BACKWARD:
                logic.Backward();
                break;
            case FASTFORWARD:
                logic.FastForward();
                break;
            case FREELY:
                logic.Freely();
                break;
            default:
                break;
        }
    }
    // Wants to cross///////////////////////
    private void TransToWantsToCross()
    {

        EditableGoal = currentGoal.previous.GetGoalPosition();
        state = WANTSTOCROSS;
    }
    private void WantsToCross()
    {
        times.UpdateTime((CrossingGoal)currentGoal, info);
        if (times.ttcCar > 2f)
        {
            TransToForward();
        }
        else
        {
            MoveOnSidewalk();
        }
    }

    //public override void Dismiss(Goal goal)
    //{
    //    Goal tmp = goal.previous;
    //    Vector3 change = ((CrossingGoal)tmp).crossingPoint.SafetyPosition - info.GetPosition();
    //    while (tmp.next == null)
    //    {
    //        ((CrossingGoal)tmp).crossingPoint.SafetyPosition += change;
    //    }
    //}
    private Vector3 dir;
    private void MoveOnSidewalk()
    {


        ///Logic outline
        ///-- Check how many cars
        /// If more than two - the weight gets really low
        /// Check both left and right movement directions
        /// Fixed weight value for going left or right depending on the position of the goal. 
        /// Both weigths can be zero
        int numberOfCars = Mathf.Clamp(((CrossingGoal)currentGoal).NumberOfCars(),2,100);
        ///Should never have 0 number of cars. If we do something is pretty fucked up in here.
        float weightCar = 2f/(float)numberOfCars;

        if (weightCar < 0.99f)
        {
            //Consider left right.
        }
        //Debug.Log("move on sidewalk");
        ((CrossingGoal)currentGoal.previous).crossingPoint.SafetyPosition = info.GetPosition();
       
        dir = ((CrossingGoal)currentGoal.previous).crossingPoint.GetSidewalkDirNew(info.GetPosition(),
            referencePoint);
        Debug.DrawRay(info.GetPosition(), dir, Color.red, 1);
        MoveCurrentGoal(dir, referencePoint);
    }

    private bool MoveCurrentGoal(Vector3 dir, Vector3 originalGoal)
    {
        dir = dir.normalized;
        float dis1 = (originalGoal - EditableGoal).magnitude;
        float dis2 = (originalGoal - EditableGoal + dir).magnitude;
        float dis3 = (originalGoal - EditableGoal - dir).magnitude;

        if (dis3 < dis1)
        {

            if (!CheckNewPositionVisibility(info.GetPosition() + dir * 4, originalGoal) || CheckStillInRoad(dir))
                return false;

            //mContext.goal = mContext.goal - dir;       
           EditableGoal = info.GetPosition() + dir * 2;
            //reference.stateMachine.goalNeeds.SetGoal(reference.stateMachine.EditableGoal);
            return true;
        }
        if (dis2 < dis1)
        {
            if (!CheckNewPositionVisibility(info.GetPosition() - dir * 4, originalGoal) || CheckStillInRoad(-dir))
                return false;
            //mContext.goal = mContext.goal + dir;
            EditableGoal = info.GetPosition() - dir * 2;
            //reference.stateMachine.goalNeeds.SetGoal(reference.stateMachine.EditableGoal);
            return true;
        }
        return false;
    }
    private bool CheckNewPositionVisibility(Vector3 newPos,Vector3 goal)
    {
        RaycastHit hit;
        
        if(Physics.Raycast(newPos,goal-newPos,out hit, (goal - newPos).magnitude,1<<LayerMask.NameToLayer("AreaCasting"))){
            return false;
        }
        return true;
    }
    private bool CheckStillInRoad(Vector3 dir)
    {
        if (((CrossingGoal)currentGoal.previous).crossingPoint.Type == CrossingPoint.CrossingPointType.START)
        {
            return ((CrossingGoal)currentGoal).crossingPoint.CheckPositionOutsideRoad(info.GetPosition() + dir * 4);
        }
        return ((CrossingGoal)currentGoal.previous).crossingPoint.CheckPositionOutsideRoad(info.GetPosition() + dir * 4);
    }

    //End wants to cross////////////////////
    private void TransToForward()
    {
        //Debug.Log("GOing Forward");
        //Vector3 change=
        ((CrossingGoal)currentGoal).crossingPoint.FindNewSafetyPosition(info.GetPosition());
        EditableGoal = ((CrossingGoal)currentGoal).crossingPoint.SafetyPosition;
        state = FORWARD;
    }
    private void Forward()
    {
        times.UpdateTime((CrossingGoal)currentGoal, info);
        //float ttcCar = ((CrossingGoal)currentGoal).MostImportantCar(info);

        //float ttFront= (((CrossingGoal)currentGoal).crossingPoint.SafetyPosition
        //    - info.GetPosition()).magnitude/info.GetComfortVelocity().magnitude;
        //float ttFrontFast= (((CrossingGoal)currentGoal).crossingPoint.SafetyPosition
        //    - info.GetPosition()).magnitude / info.GetMaximumVelocity().magnitude;
        //float ttBack = (((CrossingGoal)currentGoal.previous).crossingPoint.SafetyPosition
        //    - info.GetPosition()).magnitude / info.GetComfortVelocity().magnitude;
        //Debug.Log("ttccar:" + times.ttcCar + "|ttcFront:" + times.ttFront
        //    + "ttBack:" + times.ttBack);
        if (times.ttcCar < times.ttFront)
        {
            if (times.ttFrontFast < times.ttBack) ;
                //TransToBackward();
            else
                TransToFastForward();
            return;
        }
        if(times.ttcCar >(times.ttFront +1.5f))
        {
            TransToWantsFreely();
        }
        //if (((CrossingGoal)currentGoal).MostImportantCar(info) > 4f)
        //{
        //    Debug.Log("Faster");
        //    TransToFastForward();
        //}
    }

    //Start Backward//////////////////////////
    private void TransToBackward()
    {
        Debug.Log("GOing backwards");       
        ((CrossingGoal)currentGoal.previous).crossingPoint.FindNewSafetyPosition(info.GetPosition(),((CrossingGoal)currentGoal).crossingPoint);
        EditableGoal = ((CrossingGoal)currentGoal.previous).crossingPoint.SafetyPosition;
        state = BACKWARD;
    }

    private void Backward()
    {
       
        if(ReachedEditableGoal())
        {
            TransToWantsToCross();
            return;
        }
        times.UpdateTime((CrossingGoal)currentGoal, info);
        Debug.Log("ttccar:" + times.ttcCar + "|ttcFront:" + times.ttFront
            + "ttBack:" + times.ttBack);
        Debug.Log("back:" + times.ttBack + "| Front:" + times.ttFrontFast);
        if (times.ttcCar < times.ttFront && times.ttcCar < times.ttBack)
        {
            if (times.ttFront < times.ttBack) ;
            else
                TransToFastForward();
            return;
        }
        if(times.ttcCar > (times.ttFront)&& times.ttcCar>2f)
        {
            TransToForward();
        }
    }
    
    private bool ReachedEditableGoal()
    {
        if ((editableGoal - info.GetPosition()).magnitude < 0.2f)
        {
            return true;
        }
        return false;
    }

    //End Backward/////////////////////////////

    private void TransToFastForward()
    {
        Debug.Log("GOing FastForward");

        ((CrossingGoal)currentGoal).crossingPoint.FindNewSafetyPosition(info.GetPosition());
        EditableGoal = ((CrossingGoal)currentGoal).crossingPoint.SafetyPosition;
        info.SetMaxVelocity();
        state = FASTFORWARD;
    }

    private void FastForward()
    {
        times.UpdateTime((CrossingGoal)currentGoal, info);
        if(times.ttcCar < times.ttFront)
        {
            if (times.ttFrontFast < times.ttBack) ;
                //TransToBackward();
            return;
        }
        if(times.ttcCar > (times.ttFront) && times.ttcCar > 2f)
        {
            TransToForward();
        }
    }
    private void TransToWantsFreely()
    {
        //Debug.Log("Freely");
        //Debug.Log(editableGoal);
        ((CrossingGoal)currentGoal).crossingPoint.FindNewSafetyPosition(info.GetPosition());
        ((CrossingGoal)currentGoal).crossingPoint.FindIntersectionFreely(info.GetPosition(),
            currentGoal.GetFinalGoal());
        EditableGoal = ((CrossingGoal)currentGoal).crossingPoint.SafetyPosition;
        //Debug.Log(editableGoal);
        state = FREELY;
    }
    private void Freely()
    {
        times.UpdateTime((CrossingGoal)currentGoal,info);
        if (times.ttcCar < times.ttFront)
        {
            if (times.ttFrontFast < times.ttBack) ;
                //TransToBackward();
            else
                TransToFastForward();
            return;
        }
        if (times.ttcCar < (times.ttFront+3))
        {
            TransToForward();
        }
    }

    public void IntermediateGoals(List<CrossingPoint> crossingPoints)
    {
        CrossingGoal crossingGoal;
        Goal tmpGoal = currentGoal;
        int i = 0;
        foreach (var item in crossingPoints)
        {
            crossingGoal = new CrossingGoal(item);
            tmpGoal.next = crossingGoal;
            tmpGoal.next.previous = tmpGoal;
            tmpGoal = crossingGoal;
            new GameObject().transform.position = tmpGoal.GetGoalPosition();
            i++;
        }
        tmpGoal.next = endGoal;
        currentGoal = currentGoal.next;
        stateMachine = null;
        goalNeeds.SetGoal(currentGoal.GetGoalPosition());
        //if (stateMachine == null)
        //{
        //    stateMachine = new CrossingStateMachine();
        //    stateMachine.Reset(currentGoal);
        //}
        //else
        //    stateMachine.Reset(currentGoal);
    }
    private int CheckGoalReached()
    {
        if (currentGoal.ReachedGoal(info.GetPosition()))
        {
            if ( currentGoal.next != null)
            {
                currentGoal = currentGoal.next;
                if (currentGoal==endGoal)
                {
                    //stateMachine = null;
                    currentGoal.previous = null;
                    currentGoal.next = null;
                    Debug.Log("Current goal:"+ currentGoal.GetGoalPosition());
                    Debug.Log("Final goal:" + currentGoal.GetFinalGoal());

                    goalNeeds.SetGoal(currentGoal.GetGoalPosition());
                    return 1;
                }
                if (stateMachine == null)
                {
                    //stateMachine = new CrossingStateMachine(info);
                    //stateMachine.goalNeeds = goalNeeds;
                }
                //stateMachine.Reset(currentGoal);
                ((CrossingGoal)currentGoal).crossingPoint.FindNewSafetyPosition(info.GetPosition());
                goalNeeds.SetGoal(currentGoal.GetGoalPosition());
            }
            else
            {
                Debug.Log("Blis gib new goal");
                return 2;
            }
            TransToWantsToCross();
            //state = WANTSTOCROSS;
            return 0;
        }
        return 0;
    }
    public void OriginalGoal(Vector3 position)
    {
        currentGoal = endGoal = new Goal(position);
        endGoal.end = true;
    }
    public void CreateStateMachine()
    {
    }
    public Vector3 GetCurrentGoalPosition()
    {
        return currentGoal.GetGoalPosition();
    }

    public CrossingGoal GetIntermediateGoal()
    {
        return (CrossingGoal) currentGoal;
    }

    public Vector3 GetAgentPosition()
    {
        return info.GetPosition();
    }

    public Vector3 Velocity()
    {
        return info.GetCurrentVelocity();
    }

    public int State()
    {
        return state;
    }
    //private bool DoesMyPathCrossTheRoad()
    //{
    //    Vector3 temp; SimpleGraphRoadNetwork.Edge edge;

    //    if (context.graphRoadNetwork.FindIntersectionOfPathWithRoads(transform, out temp, out edge, out details))
    //    {
    //        if (details.sideWalkDirection != Vector3.zero && Intersection.CheckIfLeft(transform, details.GetPoint1(), details.GetPoint2()))
    //        {
    //            //Debug.Log("Wrong direction");
    //            return false;
    //        }
    //        //Debug.Log(details);
    //        //Debug.Log("Found with new algo");
    //        //edge.FindNearestPoint(transform.position, out temp);
    //        //new GameObject().transform.position = details.intersection;
    //        Debug.DrawRay(details.intersection, details.GetPerpendicular().normalized * 5, Color.green);

    //        cache.ShiftFirstToLast();
    //        context.details = details;
    //        details.crossingPoints.Clear();
    //        details.FindInBetweenNodes();
    //        details.mAgent = this;
    //        foreach (var item in details.crossingPoints)
    //        {
    //            cache.AddIntermediateGoal(item.SafetyPosition);
    //            //new GameObject().transform.position = item.SafetyPosition;
    //        }
    //        cache.ShiftFirstToLast();
    //        return true;
    //    }

    //}
}

public class TimeToCollision
{
    public float ttcCar;
    public float ttFront;
    public float ttFrontFast;
    public float ttBack;

    public void UpdateTime(CrossingGoal currentGoal,IEntity info)
    {
        ttcCar = ((CrossingGoal)currentGoal).MostImportantCar(info);
        ttFront = (((CrossingGoal)currentGoal).crossingPoint.SafetyPosition
        - info.GetPosition()).magnitude / info.GetComfortVelocity();
        ttFrontFast = (((CrossingGoal)currentGoal).crossingPoint.SafetyPosition
        - info.GetPosition()).magnitude / info.GetMaximumVelocity();
        ttBack = (((CrossingGoal)currentGoal.previous).crossingPoint.SafetyPosition
        - info.GetPosition()).magnitude / info.GetComfortVelocity();
        if (Input.GetKey(KeyCode.Alpha1))///Make him stop at crossing, and go Fast
        {
            ttcCar = 1f;
            ttFront = 1.5f;
            ttFrontFast = 1.2f;
            ttBack = 2f;
        }
        else if (Input.GetKey(KeyCode.Alpha2))///Make him stop at crossing and go back
        {
            ttcCar = 1f;
            ttFront = 1.5f;
            ttFrontFast = 1.2f;
            ttBack = 1.1f;
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {

        }


        
    }
}


public class CrossingStateMachine {
    
    private CrossingState state = null;
    private CrossingGoal goal;
    public ICrossingAction currentAction { private get; set;}
    private Vector3 editableGoal;
    private WantsToCross stWantsToCross;
    private CrossingForward stForward;
    private CrossingBackward stBackward;
    public IEntity info {get; private set;}
    public CrossingGoal Goal
    {
        get
        {
            return goal;
        }

        set
        {
            goal = value;
        }
    }
    private CrossingState tmpState;
    public IGoalNeeds goalNeeds { get; set; }

    public Vector3 EditableGoal
    {
        get
        {
            return editableGoal;
        }

        set
        {
            if (goalNeeds != null)
            {
                goalNeeds.SetGoal(value);
            }
            editableGoal = value;
        }
    }

    public void RunState()
    {
        if ((tmpState=state.CheckTransitions()) != state)
        {
            state = tmpState;
            if (currentAction == null)
                return;
            currentAction.Dismiss(Goal);
            currentAction = null;
        }
        if(currentAction != null)
        {
            ((AbstrAction)currentAction).info = info;
            currentAction.Execute(Goal);
        }
    }
    public CrossingStateMachine(IEntity info)
    {
        this.info = info;
        stWantsToCross = new WantsToCross(this);
        stForward = new CrossingForward(this);
        stBackward = new CrossingBackward(this);
        state = stWantsToCross;
        stWantsToCross.AddTransition(new GapTransition(stWantsToCross,new MoveOnSidewalk(stWantsToCross)),stForward);
        stForward.AddTransition(new FastBackwardTransition(stForward,new MoveBackwards(stForward)), stBackward);
        stForward.AddTransition(new FastBackwardTransition(stForward, new MoveBackwards(stForward)), stBackward);

        stBackward.AddTransition(null, stWantsToCross);
        stBackward.AddTransition(null, stForward);
    }
    
    public CrossingGoal GetDangerousCrossing()
    {
        return (CrossingGoal)goal.previous;
    }

    public void Reset(Goal cgoal)
    {
        state = stWantsToCross;
        goal = (CrossingGoal)cgoal;
        currentAction = null;
        tmpState = stWantsToCross;
        EditableGoal = goal.crossingPoint.previousCrossingPoint.SafetyPosition;
        //goalNeeds.SetGoal(EditableGoal);
    }
}

public abstract class CrossingState
{
    protected Dictionary<CrossingState,ICrossingTransitionValues> transitions=new Dictionary<CrossingState, ICrossingTransitionValues>();
    public CrossingStateMachine stateMachine { get; private set; }
    public CrossingState(CrossingStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void AddTransition(ICrossingTransitionValues transition, CrossingState state)
    {
        transitions.Add(state, transition);
    }

    public abstract CrossingState CheckTransitions();
}
public class WantsToCross : CrossingState
{
    public float threshold = 7.5f;
    private ICrossingAction moveOnSideWalk;
    public WantsToCross(ICrossingTransitionValues transition,CrossingState crossingForward, CrossingStateMachine stateMachine):base( stateMachine)
    {
        transitions.Add(crossingForward, transition);
        moveOnSideWalk =  new MoveOnSidewalk(this);
    }
    public WantsToCross(CrossingStateMachine stateMachine) : base(stateMachine)
    {
        moveOnSideWalk = new MoveOnSidewalk(this);
    }

    public override CrossingState CheckTransitions()
    {
        foreach (var item in transitions)
        {
            if (item.Value.TransitionValue() > threshold)
            {
                return item.Key;
            }
        }///Use actions properly via current action.
        stateMachine.currentAction = moveOnSideWalk;
        return this;
    }

}

public class CrossingForward : CrossingState
{
    public CrossingForward(ICrossingTransitionValues transition, CrossingState crossingBackward, CrossingStateMachine stateMachine) : base(stateMachine)
    {
        transitions.Add(crossingBackward, transition);
    }
    public CrossingForward(CrossingStateMachine stateMachine) : base(stateMachine)
    {

    }

    public override CrossingState CheckTransitions()
    {


        return null;
    }
}
public class CrossingFast : CrossingState
{
    public CrossingFast(ICrossingTransitionValues transition, CrossingState
      crossingForward, CrossingStateMachine stateMachine) : base(stateMachine)
    {
        transitions.Add(crossingForward, transition);
    }
    public CrossingFast(CrossingStateMachine stateMachine) : base(stateMachine)
    {

    }
    public override CrossingState CheckTransitions()
    {
        throw new System.NotImplementedException();
    }
}

public class CrossingFreely : CrossingState {

    public CrossingFreely(ICrossingTransitionValues transition, CrossingState
      crossingForward, CrossingStateMachine stateMachine) : base(stateMachine)
    {
        transitions.Add(crossingForward, transition);
    }
    public CrossingFreely(CrossingStateMachine stateMachine) : base(stateMachine)
    {

    }
    public override CrossingState CheckTransitions()
    {
        throw new System.NotImplementedException();
    }
}


public class CrossingBackward : CrossingState
{
    public CrossingBackward(ICrossingTransitionValues transition, CrossingState 
        crossingForward, CrossingStateMachine stateMachine) : base(stateMachine)
    {
        transitions.Add(crossingForward, transition);
    }
    public CrossingBackward(CrossingStateMachine stateMachine) : base(stateMachine)
    {

    }

    public override CrossingState CheckTransitions()
    {
        throw new System.NotImplementedException();
    }
}

public class CrossingGoal : Goal
{
    public CrossingPoint crossingPoint;

    public CrossingGoal(CrossingPoint crossingPoint)
    {
        this.crossingPoint = crossingPoint;
        position = crossingPoint.SafetyPosition;
    }
    public CrossingGoal(CrossingPoint crossingPoint, Goal previous, Goal next)
    {
        this.crossingPoint = crossingPoint;
        this.previous = previous;
        this.next = next;
        position = crossingPoint.SafetyPosition;
    }
    
    public override bool ReachedGoal(Vector3 agentPosition)
    {
        position = crossingPoint.SafetyPosition;
       return base.ReachedGoal(agentPosition);
    }
    public override Vector3 GetGoalPosition()
    {
        return base.GetGoalPosition();
    }
    private MovingEntity car;
    public float MostImportantCar(IEntity info)///previous crossing point is used because we always think of next step
    //e.g wants to cross a certain lane, crossing point goal is current, but i have to look for cars in previous goal.
    {
        car = crossingPoint.previousCrossingPoint.laneToObserveForCars.GetMostImportantCar
           (info.GetPosition(), crossingPoint.previousCrossingPoint);
        if (car == null)
            return 5f;
        return (car.transform.position - info.GetPosition()).magnitude / car.Velocity.magnitude;
    }
    /// <summary>
    /// Gets the number of cars for my current goal.(Dont use previous on call, it is handled in this function).
    /// </summary>
    /// <returns></returns>
    public int NumberOfCars() {

        return crossingPoint.previousCrossingPoint.laneToObserveForCars.MovingEntitiesOnLane.Count;
    }
}

public interface ICrossingAction
{
    void Execute(Goal goal);
    void Dismiss(Goal goal);
}


public abstract class AbstrAction : ICrossingAction
{
    protected CrossingState reference;
    public IEntity info { set; protected get; }
    protected bool once = true;

    public AbstrAction(CrossingState reference)
    {
        this.reference = reference;
    }

    public abstract void Dismiss(Goal goal);
    public abstract void Execute(Goal goal);
}
public class MoveBackwards : AbstrAction
{
    public MoveBackwards(CrossingState reference) : base(reference)
    {

    }

    public override void Dismiss(Goal goal)
    {
        once = true;
    }

    public override void Execute(Goal goal)
    {
        ///Apo to current position, find the proper point at a safe space to walk on and put the editable
        ///goal their
        ///
        if (!once)
            return;
        ((CrossingGoal)goal.previous).crossingPoint.FindNewSafetyPosition(info.GetPosition());
        reference.stateMachine.EditableGoal= ((CrossingGoal)goal.previous).crossingPoint.SafetyPosition;
        once = false;
    }
}

public class MoveFreely : AbstrAction
{

    public MoveFreely(CrossingState reference):base(reference)
    {
    }

    
    public override void Dismiss(Goal goal)
    {
        ((CrossingGoal)goal).crossingPoint.FindNewSafetyPosition(reference.stateMachine.info.GetPosition());
        reference.stateMachine.EditableGoal = ((CrossingGoal)goal).crossingPoint.SafetyPosition;

        once = true;
    }
    
    public override void Execute(Goal goal)
    {
        if (!once)
            return;
        ((CrossingGoal)goal).crossingPoint.FindIntersectionFreely
            (reference.stateMachine.info.GetPosition(), goal.GetFinalGoal());
        reference.stateMachine.EditableGoal = ((CrossingGoal)goal).crossingPoint.SafetyPosition;
       once = false;
    }
}
public class MoveFaster: AbstrAction
{
    public MoveFaster(CrossingState reference) : base(reference)
    {
    }
    
    public override void Dismiss(Goal goal)
    {
        reference.stateMachine.info.SetComfortVelocity();
        once = true;
    }

    public override void Execute(Goal goal)
    {
        if (!once)
            return;
        reference.stateMachine.info.SetMaxVelocity();
        once = false;
    }
}

public class MoveOnSidewalk : AbstrAction
{
    public MoveOnSidewalk(CrossingState reference) : base(reference)
    {
    }
    public override void Dismiss(Goal goal)
    {
        Goal tmp = goal.previous;
        Vector3 change = ((CrossingGoal)tmp).crossingPoint.SafetyPosition - info.GetPosition();
        while (tmp.next == null)
        {
            ((CrossingGoal)tmp).crossingPoint.SafetyPosition += change;
        }
    }
    private Vector3 dir;
    public override void Execute(Goal goal)
    {
        dir=((CrossingGoal)goal).crossingPoint.GetSidewalkDirNew(info.GetPosition(), goal.GetFinalGoal());
        MoveCurrentGoal(dir, goal.GetFinalGoal());
    }

    private bool MoveCurrentGoal(Vector3 dir,Vector3 originalGoal)
    {
        dir = dir.normalized;
        float dis1 = (originalGoal - reference.stateMachine.EditableGoal).magnitude;
        float dis2 = (originalGoal - reference.stateMachine.EditableGoal+dir).magnitude;
        float dis3 = (originalGoal - reference.stateMachine.EditableGoal-dir).magnitude;
        if(dis3 < dis1)
        {
            //mContext.goal = mContext.goal - dir;       
            reference.stateMachine.EditableGoal = info.GetPosition() + dir * 2;
            //reference.stateMachine.goalNeeds.SetGoal(reference.stateMachine.EditableGoal);
            return true;
        }
        if(dis2 < dis1)
        {
            //mContext.goal = mContext.goal + dir;
            reference.stateMachine.EditableGoal = info.GetPosition() - dir * 2;
            //reference.stateMachine.goalNeeds.SetGoal(reference.stateMachine.EditableGoal);

            return true;
        }
        return false;
    }
}

/// 
//dir= mContext.details.GetSidewalkDirNew().normalized;
//            if (MoveCurrentGoal(dir)){
//                mContext.agent.agentType.SetGoalPosition(mContext.goal);
//            }
//        }

//        /// <summary>
//        /// This will move the current goal across the sidewalk if efficient.
//        /// </summary>
//        private bool MoveCurrentGoal(Vector3 dir)
//{
//    float dis1 = (mContext.originalGoal - mContext.goal).magnitude;
//    float dis2 = (mContext.originalGoal - (mContext.goal + dir)).magnitude;
//    float dis3 = (mContext.originalGoal - (mContext.goal - dir)).magnitude;
//    if (originalSidewalkGoal == Vector3.zero)//Accepts a small inaccuracy
//    {
//        originalSidewalkGoal = mContext.goal;
//    }
//    if (dis3 < dis1)
//    {
//        //mContext.goal = mContext.goal - dir;       
//        mContext.goal = mContext.agent.transform.position - dir * 2;
//        return true;
//    }
//    if (dis2 < dis1)
//    {
//        //mContext.goal = mContext.goal + dir;
//        mContext.goal = mContext.agent.transform.position + dir * 2;
//        return true;
//    }
//    return false;
//}

//public override void Stop(IContext context)
//{
//    Debug.Log("Does nothing");
//    Debug.DrawRay(mContext.goal, mContext.goal - originalSidewalkGoal, Color.black);
//    mContext.agent.Cache.ModifyPointAcrossTheRoad(mContext.agent.State, mContext.goal - originalSidewalkGoal);
//    originalSidewalkGoal = Vector3.zero;
//    //throw new System.NotImplementedException();
//}
/// 
public interface ICrossingTransitionValues {

    float TransitionValue();
    ICrossingAction GetAction();
}

public abstract class CrossingStateTransition : ICrossingTransitionValues
{
    protected CrossingState stateMachine;
    private ICrossingAction action;
    public CrossingStateTransition(CrossingState stateMachine,ICrossingAction action)
    {
        this.stateMachine = stateMachine;
        this.action = action;
    }

    public ICrossingAction GetAction()
    {
        return action;
    }

    public abstract float TransitionValue();

}

public class GapTransition : CrossingStateTransition
{
    public GapTransition(CrossingState stateMachine, ICrossingAction action) : base(stateMachine,action)
    {
    }
    public override float TransitionValue() 
    {
        Debug.Log(stateMachine.stateMachine.Goal.MostImportantCar(stateMachine.stateMachine.info));
        //CrossingGoal goal = stateMachine.GetDangerousCrossing();
        return stateMachine.stateMachine.Goal.MostImportantCar(stateMachine.stateMachine.info);
    }
}
public class FastForwardTransition : CrossingStateTransition
{
    public FastForwardTransition(CrossingState stateMachine, ICrossingAction action) : base(stateMachine, action)
    {
        
    }
    public override float TransitionValue()
    {
        return (stateMachine.stateMachine.Goal.crossingPoint.SafetyPosition
            - stateMachine.stateMachine.info.GetPosition()).magnitude;
    }
}
public class FastBackwardTransition : CrossingStateTransition
{
    public FastBackwardTransition(CrossingState stateMachine, ICrossingAction action) : base(stateMachine, action)
    {

    }
    public override float TransitionValue()
    {
        return (((CrossingGoal)stateMachine.stateMachine.Goal.previous).crossingPoint.SafetyPosition
            - stateMachine.stateMachine.info.GetPosition()).magnitude;
    }
}

