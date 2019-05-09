using RiseProject.Tomis.DataHolders;
using System;
using UnityEngine;

[Serializable]
public abstract class FunctionalScriptableObjectWithNetworkDataAccess<T> : ScriptableObject, IRunable<T>
{
    [SerializeField]
    private SumoNetworkData _sumoNetworkData;
    [SerializeField]
    private SumoCommands _sumoCommands;
    [SerializeField]
    private TransformNetworkData _transformNetworkData;

    protected SumoNetworkData SumoNetworkData
    {
        get => _sumoNetworkData;
        private set => _sumoNetworkData = value;
    }
    protected TransformNetworkData TransformNetworkData
    {
        get => _transformNetworkData;
        set => _transformNetworkData = value;
    }
    protected SumoCommands SumoCommands
    {
        get => _sumoCommands;
        private set => _sumoCommands = value;
    }

    public abstract void Run();
    public abstract void Run(T t);
}

internal interface IRunable <T>
{
    void Run();
    void Run(T t);
}