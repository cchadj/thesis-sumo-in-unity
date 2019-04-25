using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomis.Utils.Unity;

public class TaskManager : SingletonMonoBehaviour<TaskManager>
{
    private Queue<Action> ActionsToRunInMainThread { get; } = new Queue<Action>();

    private void Update()
    {
        while (ActionsToRunInMainThread.Any()) 
            ActionsToRunInMainThread.Dequeue()();
    }

    /// <summary>
    /// Queue Actions that do Unity things such as changing the transform to be called in the main thread 
    /// </summary>
    /// <param name="action"></param>
    public void QueueAction(Action action) => ActionsToRunInMainThread.Enqueue(action);

    public Task StartAsATask(Action function, object functionLock = null)
    {
        Task t;
        if (functionLock != null)
            t = new Task(() =>
            {
                lock (functionLock) function();
            });
        else
        {
            t = new Task(function);
        }

        t.Start();
        return t;
    }

    public static Task StartAsRepeatingFunction(Action function, int milliSeconds, object functionLock)
    {
        var task = new Task(() =>
        {
            var threadSleepCheckSw = new Stopwatch();
            var simStepSw = new Stopwatch();

            while (true)
            {
                int simStepTime;
                lock (functionLock)
                {
                    simStepSw.Restart();
                    function();
                    simStepSw.Stop();
                    simStepTime = (int) simStepSw.ElapsedMilliseconds;
                }

                var sleepTime = milliSeconds - simStepTime;

                threadSleepCheckSw.Restart();
                Thread.Sleep(sleepTime > 0 ? sleepTime : 0);
                threadSleepCheckSw.Stop();

                //UnityEngine.Debug.Log(threadSleepCheckSw.ElapsedMilliseconds);
            }

            // ReSharper disable once FunctionNeverReturns
        });
        task.Start();
        return task;
    }
}
