using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MyEventArgs
{

    public readonly string nam;

    public MyEventArgs(string name)
    {
        nam = name;
    }

}

public class TestPerformanceOfEvents : MonoBehaviour
{
    public int numberOfIterations;

    private event EventHandler eventWithNoArgs; 
    private event EventHandler<MyEventArgs> eventWithArgs; 
    
    [ReadOnly] public long fieldAccessTime = 0;
    [ReadOnly] public long eventsNoArgsTime = 0;
    [ReadOnly] public long eventsWithArgsTime = 0;
    [ReadOnly] public long eventsWithMethodTime = 0;
    [ReadOnly] public long globalTime = 0;
    
    public string nam = "ha";
    void Start()
    {
        var globalSw = new Stopwatch();
        globalSw.Start();
        
        eventWithNoArgs += HandleEventNoArgs;
        eventWithArgs += HandleEventArgs;
        
        var sw = new Stopwatch();

        const int n = 10;
        for (int i = 0; i < n; i++)
        {
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var a = nam;
            }
            sw.Stop();
            fieldAccessTime += sw.ElapsedMilliseconds;

            //No Args
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                eventWithNoArgs?.Invoke(this, EventArgs.Empty);
            }
            sw.Stop();
            eventsNoArgsTime += sw.ElapsedMilliseconds;
            
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                OnEventWithNoArgs();
            }
            sw.Stop();
            eventsWithMethodTime += sw.ElapsedMilliseconds;
            
//            //Args
//            sw.Restart();
//            for (int j = 0; j < numberOfIterations; j++)
//            {
//                eventWithArgs?.Invoke(this, new MyEventArgs(nam));
//            }
//            sw.Stop();
//            eventsWithArgsTime += sw.ElapsedMilliseconds;
//            
//            sw.Restart();
//            for (int j = 0; j < numberOfIterations; j++)
//            {
//                OnEventWithArgs();
//            }
//            sw.Stop();
//            eventsWithMethodTime += sw.ElapsedMilliseconds;

        }

        fieldAccessTime /= n;
        eventsNoArgsTime /= n;
        eventsWithArgsTime /= n;
        eventsWithMethodTime /= n;
        
        globalSw.Stop();
        globalTime = globalSw.ElapsedMilliseconds;
    }

    protected void OnEventWithNoArgs()
    {
        eventWithNoArgs?.Invoke(this, EventArgs.Empty);
    }
    
    protected void OnEventWithArgs()
    {
        eventWithArgs?.Invoke(this, new MyEventArgs(nam));
    }
    void HandleEventNoArgs(object sender, EventArgs e)
    {
        var a = this.nam;
    }
    
    void HandleEventArgs(object sender, MyEventArgs e)
    {
        var a = e.nam;
    }
}
