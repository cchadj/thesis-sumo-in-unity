using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    private State<T> currentState;
}

public abstract class State<T>
{
    protected T node;
    protected IEntity info;
    protected List<Transition> transitions = new List<Transition>();
    
    public abstract void Logic();

    public void AddTransition(Transition transition)
    {
        transitions.Add(transition);
    }
    public void RemoveTransition(Transition toRemove)
    {
        transitions.Remove(toRemove);
    }
}

public interface Transition
{
    bool CheckTransition();
}
