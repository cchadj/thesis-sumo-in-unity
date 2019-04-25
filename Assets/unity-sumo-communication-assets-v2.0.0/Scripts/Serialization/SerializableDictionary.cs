using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        this.Clear();
        Type keyListType = keys.GetType().GetTypeInfo().GenericTypeArguments[0];
        Type valueListType = values.GetType().GetTypeInfo().GenericTypeArguments[0];
        if (keys.Count != values.Count)
            throw new System.Exception(
                $"there are {keys.Count} keys and {values.Count} values after deserialization." +
                $" Make sure that both key and value types are serializable." +
                $" Key Type is {keyListType.ToString()} and value Type is {valueListType.ToString()}");

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}