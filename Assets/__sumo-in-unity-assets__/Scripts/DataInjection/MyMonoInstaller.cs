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
    [SerializeField] private InputManager inputManager;

    [Header("Canvases")]
    [SerializeField] private VehicleCanvas vehicleCanvas;

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
        
        Container.BindInterfacesAndSelfTo<InputManager>()
            .AsSingle()
            .NonLazy();
        
        // BindInstance is the same as Bind<ResultType>().FromInstance(theInstance)...
        
        //Bind Singletons. Simulation Components
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
        
        // Bind Canvases Instances 
        Container.BindInstance(vehicleCanvas)
            .AsSingle()
            .NonLazy();
        
        // Factories
        Container.BindFactory<GameObject, Car, Car.Factory>()
            .FromFactory<PrefabFactory<Car>>();


    }
}