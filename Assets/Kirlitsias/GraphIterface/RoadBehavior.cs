using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class RoadBehavior 
{

}

public class CrossingFunction
{
    private Profile mProfile;
    private float activation;
    private float previousTTC = -10;

    public Profile MProfile
    {
        set
        {
            mProfile = value;
        }
    }

    public float GetActivation()
    {
        return activation;
    }

    public void ReduceActivation()
    {
        activation -= (1 / activation) * Time.deltaTime;
    }
    public void DynamicActivation(float activationChange)
    {
        activation += activationChange;
    }
    public float TtcActivation(float ttc,float ttfront)
    {
        float maxActivation = 2f;
        if (ttc - ttfront < 0)
            return 0;
        float diff = Mathf.Clamp(ttc - ttfront, 0, 3) / 3f;
        return Mathf.Clamp(diff, 1, maxActivation);
    }
    public float AccelerationActivation(float ttc,float timePassed,float distance)
    {
        if (previousTTC == -10) {
            previousTTC = ttc;
            return 0;
        }
        float dttcolision = previousTTC - ttc;
        if(dttcolision/timePassed > 0.45f)
        {
            distance = 1/Mathf.Pow((Mathf.Clamp(distance, 10, 50)/10f),2);
            return 2 * distance;
        }
        return 0;
    }

    public float ImitationActivation()
    {
        float imitationForceOfPart = 2;//Should take proper force soon
        return mProfile.imitationRate * imitationForceOfPart;
    }

}
[System.Serializable]
public class Profile
{
    public const int YOUNG = 0;
    public const int ADULT = 1;
    public const int OLD = 2;
    public const float BASE_COMFORT_SPEED = 1.2f;
    public const float BASE_MAX_SPEED = 3f;
    public const float AVERAGE_HEIGHT=1.8f;
    public const float IMITATION_INITIAL = 1f;
    public IEntity info;
    public int age;
    public bool man;
    [SerializeField]
    private float height= AVERAGE_HEIGHT;
    public float imitationRate = IMITATION_INITIAL;
    private float distanceEstimation = 1f;
    private float speedEstimation = 1f;
    [SerializeField]
    private float comfortSpeed = BASE_COMFORT_SPEED;
    [SerializeField]
    private float maxSpeed= BASE_MAX_SPEED;
    public float AverageHeightPlusMinus = 0.2f;


    public Profile()
    {
        man = true; //TOMIS -changed to TRUE to remove error.Random can not be called from serialisation Random.Range(0, 2)==1;
        age = YOUNG; // TOMIS - changed to YOUN for same ReasonRandom.Range(YOUNG, OLD + 1);
        height = AVERAGE_HEIGHT + AverageHeightPlusMinus; // TOMIS - same reason Random.Range(AVERAGE_HEIGHT- AverageHeightPlusMinus, AVERAGE_HEIGHT+ AverageHeightPlusMinus);
        AdaptGenderRelatedCharacteristics();
        AdaptAgeRelatedCharacteristics();
        AdaptHeightRelatedCharacteristics();
    }
    private void AdaptGenderRelatedCharacteristics()
    {
        imitationRate = 1f;
        if (!man)
        {
            comfortSpeed -= 0.1f;
            maxSpeed -= 0.2f;
            imitationRate = 0.8f;
            if(age!=YOUNG)
                height -= 0.15f;
        }
    }
    private void AdaptAgeRelatedCharacteristics()
    {
        switch (age)
        {
            case YOUNG:
                comfortSpeed -= 0.1f;
                maxSpeed -= 0.3f;
                distanceEstimation -= 0.5f;
                speedEstimation -= 0.5f;
                height -= 0.1f; // TOMIS : Remove because origin Random.Range(0.1f, 0.6f);
                imitationRate = IMITATION_INITIAL * 2f;
                break;
            case OLD:
                comfortSpeed -= 0.2f;
                maxSpeed -= 0.8f;
                distanceEstimation -= 0.25f;
                speedEstimation -= 0.25f;
                break;
            default:
                break;
        }
    }
    
    private void AdaptHeightRelatedCharacteristics()
    {
        float diffFromAverage = -AVERAGE_HEIGHT+height;
        //float sign = Mathf.Sign(diffFromAverage);//positive means shorter than average.
        //float absDifference = Mathf.Abs(diffFromAverage);
        comfortSpeed += diffFromAverage / 3f;
        maxSpeed += diffFromAverage / 2f;

    }

    public float ComfortSpeed
    {
        get
        {
            return comfortSpeed;
        }
    }

    public float MaxSpeed
    {
        get
        {
            return maxSpeed;
        }
   
    }


}

public class AgentTimers
{
    public static Dictionary<IEntity, float> agentDeltaTimes = new Dictionary<IEntity, float>();
    public static Dictionary<IEntity, bool> agentResets = new Dictionary<IEntity, bool>();
    public static bool GetAgentDelta(IEntity agent,out float delta)
    {
        if(agentDeltaTimes.TryGetValue(agent,out delta))
        {
            return true;
        }
        return false;
    }
    
    public static void ResetAgentTimer(IEntity agent,bool turnDic2True)
    {

        if (agentDeltaTimes.ContainsKey(agent))
        {
            agentDeltaTimes[agent] = 0;
            agentResets[agent] = turnDic2True;
        }
    }
    public static void AddTime(IEntity agent)
    {
        if (agentDeltaTimes.ContainsKey(agent))
        {
            if (agentResets[agent])
            {
                ResetAgentTimer(agent, false);
            }
            else
                agentDeltaTimes[agent] += Time.deltaTime;
        }
        else
        {
            agentDeltaTimes.Add(agent, 0);
            agentResets.Add(agent, false);
        }
    }
}

public abstract class StateLogic
{
    public abstract void Freely();
    public abstract void FastForward();
    public abstract void Backward();
    public abstract void Forward();
    public abstract void WantsToCross();
}
public class ActivationBasedStaeLogic : StateLogic
{
    public override void Backward()
    {
        throw new System.NotImplementedException();
    }

    public override void FastForward()
    {
        throw new System.NotImplementedException();
    }

    public override void Forward()
    {
        throw new System.NotImplementedException();
    }

    public override void Freely()
    {
        throw new System.NotImplementedException();
    }

    public override void WantsToCross()
    {
        throw new System.NotImplementedException();
    }
}

public class TimerBasedStateLogic : StateLogic
{
    public System.Action MoveOnSidewalk;
    public System.Action TransToForward;
    public System.Action TransToFastForward;
    public System.Action TransToWantsToCross;
    public System.Action TransToWantsFreely;
    public System.Action TransToBackward;
    public System.Func<bool> ReachedEditableGoal;
    public TimeToCollision times;

    public override void Backward()
    {
        if (ReachedEditableGoal())
        {
            TransToWantsToCross();
            return;
        }
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
        if (times.ttcCar > (times.ttFront) && times.ttcCar > 2f)
        {
            TransToForward();
        }
    }

    public override void FastForward()
    {
        if (times.ttcCar < times.ttFront)
        {
            if (times.ttFrontFast < times.ttBack) ;
            //TransToBackward();
            return;
        }
        if (times.ttcCar > (times.ttFront) && times.ttcCar > 2f)
        {
            TransToForward();
        }
    }

    public override void Forward()
    {
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
        if (times.ttcCar > (times.ttFront + 1.5f))
        {
            TransToWantsFreely();
        }
        //if (((CrossingGoal)currentGoal).MostImportantCar(info) > 4f)
        //{
        //    Debug.Log("Faster");
        //    TransToFastForward();
        //}
    }

    public override void Freely()
    {
        if (times.ttcCar < times.ttFront)
        {
            if (times.ttFrontFast < times.ttBack) ;
            //TransToBackward();
            else
                TransToFastForward();
            return;
        }
        if (times.ttcCar < (times.ttFront + 3))
        {
            TransToForward();
        }
    }

    public override void WantsToCross()
    {
        if (times.ttcCar > 3f)
        {

            TransToForward();
        }
        else
        {
            MoveOnSidewalk();
        }
    }
}