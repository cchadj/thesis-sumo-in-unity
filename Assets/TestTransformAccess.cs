using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TestTransformAccess : MonoBehaviour
{
    public int numberOfIterations;
    [ReadOnly] public long cachedTransformAccessTime = 0;
    [ReadOnly] public long directTransformAccessTime = 0;

    private Transform _transform;
    void Start()
    {

        var sw = new Stopwatch();
        _transform = GetComponent<Transform>();
        
        for (int j = 0; j < numberOfIterations; j++)
        {
            var t = _transform;
        }
        
        for (int j = 0; j < numberOfIterations; j++)
        {
            var t = transform;
        }
        
        const int n = 10;
        for (int i = 0; i < n; i++)
        {
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var t = _transform;
            }
            sw.Stop();
            cachedTransformAccessTime += sw.ElapsedMilliseconds;

            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var t = transform;
            }
            sw.Stop();
            directTransformAccessTime += sw.ElapsedMilliseconds;
        }

        cachedTransformAccessTime /= n;
        directTransformAccessTime /= n;

    }

}
