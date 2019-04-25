using UnityEngine;

namespace Tomis.Utils.Unity
{
    /// <summary>
    /// Inherit from this base class to create a singleton.
    /// e.g. public class MyClassName : Singleton&lt;MyClassName&gt; {}
    /// </summary>
    public class SingletonMonoBehaviour<T> : Singleton where T : MonoBehaviour
    {

        private static T _instance;
        /// <summary>
        /// Access singleton instance through this propriety.
        /// If no such object in scene then null is returned.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (IsShuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed. Returning null.");
                    return null;
                }

                lock (Lock)
                {
                    if (_instance == null)
                    {
                        // Search for existing instance.
                        _instance = (T)FindObjectOfType(typeof(T));
                    }
                    
                    return _instance;
                }
            }
        }


        private void OnApplicationQuit()
        {
            IsShuttingDown = true;
        }


        private void OnDestroy()
        {
            IsShuttingDown = true;
        }
    }

    public class Singleton : MonoBehaviour
    {
            // Check to see if we're about to be destroyed.
            protected static bool IsShuttingDown = false;
            protected static readonly object Lock = new object();
            
    }
}