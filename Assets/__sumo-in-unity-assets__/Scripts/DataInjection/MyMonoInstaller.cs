using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity;
using UnityEngine;
using Zenject;

public class MyMonoInstaller : MonoInstaller
{
    [SerializeField] private SumoNetworkData sumoNetworkData;
    [SerializeField] private SimulationStartupData startupData;
    [SerializeField] private SumoToUnityGameObjectMap sumoToUnityGameObjectMap;
    [SerializeField] private ApplicationManager applicationManager;
    [SerializeField] private SumoClient sumoClient;

    public override void InstallBindings()
    {
        Container.Bind<SumoCommands>()
            .AsSingle()
            .NonLazy();

        Container.Bind<SimulationState>()
            .AsSingle()
            .NonLazy();

        Container.Bind<CurrentlySelectedTargets>()
            .AsSingle()
            .NonLazy();
        
        // BindInstance is the same as Bind<ResultType>().FromInstance(theInstance)...
        Container.BindInstance(startupData)
            .AsSingle()
            .NonLazy(); 

        Container.BindInstance(sumoNetworkData)
            .AsSingle()
            .NonLazy();
        
        Container.BindInstance(sumoToUnityGameObjectMap)
            .AsSingle()
            .NonLazy();
        
        Container.BindInstance(applicationManager)
            .AsSingle()
            .NonLazy();

        Container.BindInstance(sumoClient)
            .AsSingle()
            .NonLazy();
   }
}