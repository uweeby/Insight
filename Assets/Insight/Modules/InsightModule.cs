using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public interface IServerModule
    {
        IEnumerable<Type> Dependencies { get; }
        IEnumerable<Type> OptionalDependencies { get; }

        void Initialize(InsightServer server, ModuleManager manager);
        void Initialize(InsightClient client, ModuleManager manager);
    }

    public abstract class InsightModule : MonoBehaviour, IServerModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InsightModule));

        private static Dictionary<Type, GameObject> _instances;

        private readonly List<Type> _dependencies = new List<Type>();
        private readonly List<Type> _optionalDependencies = new List<Type>();

        /// <summary>
        ///     Returns a list of module types this module depends on
        /// </summary>
        public IEnumerable<Type> Dependencies
        {
            get { return _dependencies; }
        }

        public IEnumerable<Type> OptionalDependencies
        {
            get { return _optionalDependencies; }
        }

        /// <summary>
        ///     Called by master server, when module should be started
        /// </summary>
        public virtual void Initialize(InsightServer server, ModuleManager manager)
        {
            if (server.logNetworkMessages) { Debug.LogWarning("[Module Manager] Initialize InsightServer not found for module"); }
        }
        public virtual void Initialize(InsightClient client, ModuleManager manager)
        {
            if(client.logNetworkMessages) { Debug.LogWarning("[Module Manager] Initialize InsightClient not found for module"); }
        }
        public virtual void Initialize(InsightCommon insight, ModuleManager manager)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Adds a dependency to list. Should be called in Awake or Start methods of
        ///     module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddDependency<T>()
        {
            _dependencies.Add(typeof(T));
        }

        public void AddOptionalDependency<T>()
        {
            _optionalDependencies.Add(typeof(T));
        }

        /// <summary>
        /// Returns true, if module should be destroyed
        /// </summary>
        /// <returns></returns>
        protected bool DestroyIfExists()
        {
            if (_instances == null)
                _instances = new Dictionary<Type, GameObject>();

            if (_instances.ContainsKey(GetType()))
            {
                if (_instances[GetType()] != null)
                {
                    // Module hasn't been destroyed
                    Destroy(gameObject);
                    return true;
                }

                // Remove an old module, which has been destroyed previously
                // (probably automatically when changing a scene)
                _instances.Remove(GetType());
            }

            _instances.Add(GetType(), gameObject);
            return false;
        }
    }
}
