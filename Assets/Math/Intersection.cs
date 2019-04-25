using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Intersection {

    private static bool Reverse = false;

    public static bool LineSegmentsIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersection)
    {
        intersection = Vector3.zero;
        var d = (p2.x - p1.x) * (p4.z - p3.z) - (p2.z - p1.z) * (p4.x - p3.x);
        if (d == 0.0f)
        {
            return false;
        }
        var u = ((p3.x - p1.x) * (p4.z - p3.z) - (p3.z - p1.z) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.z - p1.z) - (p3.z - p1.z) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }
        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.z = p1.z + u * (p2.z - p1.z);
        return true;
    }
    /// <summary>
    /// The two lines should have the same direction to work properly
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="B"></param>
    /// <param name="A"></param>
    /// <param name="B1"></param>
    /// <param name="A1"></param>
    /// <returns></returns>
    private static bool CheckIfBetween2Lines(Transform transform, Vector3 B,Vector3 A,Vector3 B1,Vector3 A1)
    {

        float position1 = Mathf.Sign((B.x - A.x) * (transform.position.z - A.z) - (B.z - A.z) * (transform.position.x - A.x));

        float position2 = Mathf.Sign((B1.x - A1.x) * (transform.position.z - A1.z) - (B1.z - A1.z) * (transform.position.x - A1.x)); ;

        if (position1 > 0 && position2 < 0)
        {
            return false;
        }
        if (position1 < 0 && position2 > 0)
        {
            return false;
        }
        return true;
    }
    public static bool CheckIfLeft(Transform transform, Vector3 B, Vector3 A)
    {
        Debug.DrawRay(B, B - A, Color.white);
        //new GameObject().transform.position = B;
        //new GameObject().transform.position = A;

        if (Mathf.Sign((B.x - A.x) * (transform.position.z - A.z) - (B.z - A.z) * (transform.position.x - A.x)) > 0)
        {
            if (Reverse) return false;
            return true;
        }
        if (Reverse) return true;

        return false;
    }
    public static bool CheckIfLeft(Vector3 position, Vector3 B, Vector3 A)
    {
        Debug.DrawRay(B, B - A, Color.white);
        //new GameObject().transform.position = B;
        //new GameObject().transform.position = A;

        if (Mathf.Sign((B.x - A.x) * (position.z - A.z) - (B.z - A.z) * (position.x - A.x)) > 0)
        {
            if (Reverse) return false;

            return true;
        }
        if (Reverse) return true;

        return false;
    }

    public static bool IntersectionOfLines(Vector3 p1,Vector3 p2, Vector3 l1,Vector3 l2,out Vector3 point)
    {

        //Ax + By = C
        float tmp = l1.x;
        l1.x = l1.z;
        l1.z =tmp;
        tmp = l2.x;
        l2.x = l2.z;
        l2.z = tmp;
        l1.x = -l1.x;
        l2.x = -l2.x;
        float c1 = l1.x * p1.x + l1.z * p1.z;
        float c2 = l2.x * p2.x + l2.z * p2.z;

        float delta = l1.x * l2.z - l2.x * l1.z;
        point = Vector3.zero;
        if (delta == 0)
            return false;

        float x = (l2.z * c1 - l1.z * c2) / delta;
        float y = (l1.x * c2 - l2.x * c1) / delta;

        point = new Vector3(x, (p1.y + p2.y) / 2f, y);
        return true;


        //float delta = A1 * B2 - A2 * B1;

        //if (delta == 0)
        //    throw new ArgumentException("Lines are parallel");

        //float x = (B2 * C1 - B1 * C2) / delta;
        //float y = (A1 * C2 - A2 * C1) / delta;
    }
}
