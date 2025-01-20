using Unity.Netcode;
using UnityEngine;

namespace _Ravars.Scripts.Utils
{
    public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
    {
        public static T Instance { get; protected set; }
        public static bool InstanceExists => Instance is not null;

        protected virtual void Awake()
        {
            if (InstanceExists)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = (T)this;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}