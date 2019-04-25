using RiseProject.Tomis.SumoInUnity.SumoTypes;

/// <summary>
/// Implemented on objects that can be selected.
/// Must provide TraciVariable.
/// </summary>
/// <typeparam name="T"> The type of the TraciVariable </typeparam>
public interface ISelectableTraciVariable
{
    T GetTraciVariable<T>() where T : TraCIVariable;
    bool SetTraciVariable<T>(T traciVariable) where T : TraCIVariable;
}
