using RiseProject.Tomis.DataHolders;
using UnityEngine;
using Zenject;

public class MyMonoInstaller : MonoInstaller
{
    [SerializeField] private SumoNetworkData sumoNetworkData;
    public override void InstallBindings()
    {
        Container.BindInstance(sumoNetworkData)
            .AsSingle()
            .NonLazy();
        Container.Bind<SumoCommands>()
            .AsSingle()
            .NonLazy();
    }
}