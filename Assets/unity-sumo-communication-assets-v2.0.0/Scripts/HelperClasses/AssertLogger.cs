#if UNITY_ASSERTIONS

using System.Diagnostics;
using Tomis.Utils.Unity;
using Tomis.Utils;
using UnityEngine;

[DefaultExecutionOrder(-30000)]
[CreateAssetMenu]
public class AssertLogger : SingletonMonoBehaviour<AssertLogger>
{
    private DefaultTraceListener _defaultTraceListener;
    private int _logFileCount;
    private void Awake()
    {
        var _defaultTraceListener = new DefaultTraceListener
        {
            LogFileName = FileHelper.GetAvailableFileName(
                Application.dataPath + @"\assert-logs",
                "assertion-log",
                ".txt",
                out var count)
        };
        
        Trace.Listeners.Clear();
        Trace.Listeners.Add(_defaultTraceListener);

        _logFileCount = count;
        DontDestroyOnLoad(this.gameObject);
    }
    
}
#endif