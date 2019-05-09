
namespace RiseProject.Tomis.DataContainers
{
    /// <summary>
    /// Used for GetComponent because GetComponent<TraciVariable<T>> doesn't work. 
    /// This is a workarround so I can use GetComponent<IDataHolder> and get the component I want.
    /// </summary>
    internal interface IDataHolder
    {
    }
}