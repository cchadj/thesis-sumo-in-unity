using RiseProject.Tomis.DataContainers;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "MyScriptableInstaller", menuName = "Installers/MyScriptableInstaller")]
public class MyScriptableInstaller : ScriptableObjectInstaller<MyScriptableInstaller>
{
    [SerializeField] private GameObject vehiclePrefab;
    public override void InstallBindings()
    {
        //Container.BindFactory<Car, Car.Factory>().FromSubContainerResolve().ByNewContextPrefab(vehiclePrefab);
    }
}