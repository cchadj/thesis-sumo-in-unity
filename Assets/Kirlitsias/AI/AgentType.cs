using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAgentType {

    /// <summary>
    /// Check whether the goal is near me or not.
    /// </summary>
    bool CheckDistanceToGoal();

    /// <summary>
    /// Moves the agent towards the goal using the underlying mechanism provided by the agent logic.
    /// </summary>
    void MoveTowardsGoal();
    /// <summary>
    /// Set the goal position.
    /// </summary>
    /// <param name="goal"></param>
    void SetGoalPosition(Vector3 goal);
    /// <summary>
    /// Set the goal transform.Should determine which one to keep.
    /// </summary>
    /// <param name="goal"></param>
    void SetGoalTransform(TrackedReference goal);

    /// <summary>
    /// Sets the agents underlying navigation mechanisms speed to its maximum.
    /// This in the future should be more adjustable, changing the speed using according
    /// to a dangerous value.
    /// </summary>
    void SetAgentMaximumSpeed();

    /// <summary>
    /// This function resets the agents value of speed to normal.
    /// </summary>
    void ResetAgentSpeedToNormal();
}
