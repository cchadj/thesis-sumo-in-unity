using UnityEngine;
using System.Collections;

public interface IEntity 
{
    Vector3 GetPosition();
    Vector3 GetGoal();
    Vector3 GetCurrentVelocity();
    float GetComfortVelocity();
    float GetMaximumVelocity();
    void SetMaxVelocity();
    void SetComfortVelocity();
}
