using System;
using System.Collections.Generic;

using UnityEngine;

using RiseProject.Tomis.SumoInUnity.SumoTypes;
using Tomis.Utils.Unity;

namespace RiseProject.Tomis.Util.Serializable
{
    [Serializable] public class EdgeDictionary : SerializableDictionary<string, Edge> { }

    [Serializable] public class LaneDictionary : SerializableDictionary<string, Lane> { }

    [Serializable] public class RouteDictionary : SerializableDictionary<string, Route> { }

    [Serializable] public class VehicleDictionary : SerializableDictionary<string, Vehicle> { }

    [Serializable] public class JunctionDictionary : SerializableDictionary<string, Junction> { }

    /// <summary> ID maps tou many game objects </summary>
    [Serializable] public class IDtoGameObjectsDictionary : SerializableDictionary<string, List<GameObject>> { }

    /// <summary> Maps to a single object </summary>
    [Serializable] public class IDtoGameObjectDictionary : SerializableDictionary<string, GameObject> { }
}

namespace RiseProject.Tomis.DataContainers { [Serializable] public class NetworkData : SingletonScriptableObject<SumoNetworkData> { } }



