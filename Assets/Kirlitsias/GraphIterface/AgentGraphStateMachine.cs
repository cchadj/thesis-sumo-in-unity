using System.Collections;
using System.Collections.Generic;
using RiseProject.Kirlitsias;
using UnityEngine;

public class AgentGraphStateMachine : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	//Update is called once per frame
	void Update (){
		
	}
    
}

public class GoalFollowing : State<Vector3>
{
    public override void Logic()
    {
        ///PX move towards goal.
        ///Then check Transitions?
        ///Or transitions outside?
    }
}
public class ReachedGoal : State<Vector3>
{
    public override void Logic()
    {
        ///PX move towards goal.
    }
}
public class StartIntermediateGoal : IntermediateGoal
{
    public override void Logic()
    {
        //throw new System.NotImplementedException();
    }
    public StartIntermediateGoal(CrossingPoint point) : base(point)
    {
    }
}
public class EndIntermediateGoal : IntermediateGoal
{
    public override void Logic()
    {
        //throw new System.NotImplementedException();
    }
    public EndIntermediateGoal(CrossingPoint point):base(point)
    {    }
}
public class IntermediateGoal : State<CrossingPoint>
{
    public IntermediateGoal(CrossingPoint point)
    {
        node = point;
        State = WANTS_TO_CROSS;
    }
    //protected enum IntermediateGoalStates { WANTS_TO_CROSS,CROSSING}
    protected const int WANTS_TO_CROSS = 0;
    protected const int CROSSING = 1;

    protected int State=0;
    public override void Logic()
    {
        //throw new System.NotImplementedException();
        ///Go towards
        ///Go

        switch (State)
        {
            case WANTS_TO_CROSS:
                WantsToCross();
                break;
            case CROSSING:
                Crossing();
                break;
            default:
                break;
        }
    }

    protected void WantsToCross()
    {
        QueryLane();
    }

    private void QueryLane()
    {
        if (DoICross())
            State = CROSSING;
        else
        {
            MoveOnSafeArea();
        }
    }
    
    private bool DoICross()
    {
        MovingEntity car = node.laneToObserveForCars.GetMostImportantCar(info.GetPosition(), node);
        if (car.Gap(info.GetPosition()) > 3)
        {
            return true;
        }
        return false;
    }
    private void MoveOnSafeArea()
    {
        node.GetSidewalkDirNew(info.GetPosition(),info.GetGoal());
    }

    protected void Crossing()
    {

    }
    
}