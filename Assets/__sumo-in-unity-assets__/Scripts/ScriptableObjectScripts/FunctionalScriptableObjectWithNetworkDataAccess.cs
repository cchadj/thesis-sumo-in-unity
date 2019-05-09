using RiseProject.Tomis.DataContainers;
using System;
using RiseProject.Tomis.SumoInUnity;
using UnityEngine;

[Serializable]
public abstract class FunctionalScriptableObjectWithNetworkDataAccess<T> : ScriptableObject, IRunable<T>
{
    [SerializeField]
    private SumoNetworkData _sumoNetworkData;
    [SerializeField]
    private SumoCommands _sumoCommands;
    [SerializeField]
    private SumoToUnityGameObjectMap sumoToUnityGameObjectMap;

    protected SumoNetworkData SumoNetworkData
    {
        get => _sumoNetworkData;
        private set => _sumoNetworkData = value;
    }
    protected SumoToUnityGameObjectMap SumoToUnityGameObjectMap
    {
        get => sumoToUnityGameObjectMap;
        set => sumoToUnityGameObjectMap = value;
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