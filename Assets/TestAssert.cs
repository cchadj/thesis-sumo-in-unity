
using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Diagnostics;
public class TestAssert : MonoBehaviour
{
    public string LogFileName { get; set; }
    
    // Start is called before the first frame update
    void Start()
    {

        // Create and add a new default trace listener.
        DefaultTraceListener defaultListener;
        defaultListener = new DefaultTraceListener();
        Trace.Listeners.Add(defaultListener);

       defaultListener.LogFileName = Application.dataPath + @"\log.txt";
       Assert.IsTrue(true);
       try
       {
           UnityEngine.Assertions.Assert.IsTrue(false);
       }
       catch (Exception e)
       {
           UnityEngine.Debug.LogError("UnityEngine.Assertions.Assert assertion thrown");
       }
       
       try
       {
           UnityEngine.Debug.Assert(false);
       }
       catch (Exception e)
       {
           UnityEngine.Debug.LogError("UnityEngine.Debug.Assert thrown");
       }
       
       try
       {
           System.Diagnostics.Debug.Assert(false);
           
       }
       catch (Exception e)
       {
           UnityEngine.Debug.LogError("System.Diagnostics.Debug.Assert thrown");
       }
       
       try
       {
           System.Diagnostics.Trace.Assert(false);
           
       }
       catch (Exception e)
       {
           UnityEngine.Debug.LogError("System.Diagnostics.Debug.Assert thrown");
       }
       
#if UNITY_ASSERTIONS
        Assert.IsTrue(true);
        Assert.IsTrue(true);
        try
        {
            
            Assert.IsTrue(false);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("UnityEngine.Assertions.Assert assertion thrown inside UNITY_ASSERTIONS");
        }
       
        try
        {
            UnityEngine.Debug.Assert(false);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("UnityEngine.Debug.Assert thrown inside UNITY_ASSERTIONS");
        }
       
        try
        {
            System.Diagnostics.Debug.Assert(false);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("System.Diagnostics.Debug.Assert thrown inside UNITY_ASSERTIONS");
        }

 #endif
    }
}
