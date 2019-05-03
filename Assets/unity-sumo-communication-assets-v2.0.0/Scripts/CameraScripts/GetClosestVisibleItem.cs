using UnityEngine;

public class GetClosestVisibleItem : MonoBehaviour
{
   private GetVisibleTargets visibleTarget;
   public Vector3 origin;
   public Transform bestTarget;
   
   private void Start()
   {
      visibleTarget = GetComponent<GetVisibleTargets>();
      visibleTarget.NewTargetsVisible += GetClosestItem;
   }

   
   private void GetClosestItem(object sender, VisibleTargetEventArgs e)
   {
      var size = e.Size;
      var collidedObjects = e.CollidedObjects;
      origin = e.FrustumCenter;
      
      var closestDistanceSqr = Mathf.Infinity;
      Transform closestTarget = null; 
      for(var i = 0; i < size; i++)
      {
         var potentialTarget = collidedObjects[i].transform;
         var potentialTargetPosition = potentialTarget.position;
         
         var distX = potentialTargetPosition.x - origin.x;
         var distZ = potentialTargetPosition.z - origin.z;
         var dSqrToTarget = distX * distX + distZ * distZ;
            
         if (dSqrToTarget < closestDistanceSqr)
         {
            closestDistanceSqr = dSqrToTarget;
            closestTarget = potentialTarget;
         }
      }

      bestTarget = closestTarget;

   }

   
   // Cache for efficiency 
   private static readonly Vector3 vector3Down = Vector3.down * 1000f;
   private void OnDrawGizmos()
   {
      Gizmos.DrawLine(origin, origin + vector3Down);
   }
}
