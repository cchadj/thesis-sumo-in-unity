using RiseProject.Tomis.DataHolders;
using RiseProject.Tomis.SumoInUnity;
using UnityEngine;
using Zenject;

public class MyMonoInstaller : MonoInstaller
{
    [SerializeField] private SumoNetworkData sumoNetworkData;
    [SerializeField] private SimulationStartupData startupData;
    [SerializeField] private ApplicationManager applicationManager;
    [SerializeField] private SumoClient sumoClient;

    public override void InstallBindings()
    {
        Container.Bind<SumoCommands>()
            .AsSingle()
            .NonLazy();

        Container.BindInstance(startupData)
            .AsSingle()
            .NonLazy(); 

        Container.BindInstance(sumoNetworkData)
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