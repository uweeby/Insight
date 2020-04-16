using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

namespace Insight
{
    public class ModuleManager : MonoBehaviour
    {
        NetworkClient client;
        NetworkServer server;

        public bool SearchChildrenForModule = true;

        private Dictionary<Type, InsightModule> _modules;
        private HashSet<Type> _initializedModules;

        private bool _initializeComplete;

        void Awake()
        {
            client = GetComponent<NetworkClient>();
            server = GetComponent<NetworkServer>();
        }

        void Start()
        {
            _modules = new Dictionary<Type, InsightModule>();
            _initializedModules = new HashSet<Type>();
        }

        void Update()
        {
            if (!_initializeComplete)
            {
                _initializeComplete = true;

                InsightModule[] modules = SearchChildrenForModule ? GetComponentsInChildren<InsightModule>() : FindObjectsOfType<InsightModule>();

                // Add modules
                foreach (var module in modules)
                    AddModule(module);

                // Initialize modules
                InitializeModules(client, server);
            }
        }

        public void AddModule(InsightModule module)
        {
            if (_modules.ContainsKey(module.GetType()))
            {
                throw new Exception("A module already exists in the server: " + module.GetType());
            }

            _modules[module.GetType()] = module;
        }

        public bool RemoveModule(InsightModule module)
        {
            if (_modules.ContainsKey(module.GetType()))
            {
                return _modules.Remove(module.GetType());
            }
            return false;
        }

        public bool InitializeModules(NetworkClient client, NetworkServer server)
        {
            bool checkOptional = true;

            // Initialize modules
            while (true)
            {
                bool changed = false;
                foreach (var entry in _modules)
                {
                    // Module is already initialized
                    if (_initializedModules.Contains(entry.Key))
                        continue;

                    // Not all dependencies have been initialized
                    if (!entry.Value.Dependencies.All(d => _initializedModules.Any(d.IsAssignableFrom)))
                        continue;

                    // Not all OPTIONAL dependencies have been initialized
                    if (checkOptional && !entry.Value.OptionalDependencies.All(d => _initializedModules.Any(d.IsAssignableFrom)))
                        continue;

                    // If we got here, we can initialize our module
                    if (server)
                    {
                        entry.Value.Initialize(server, this);
                        Debug.LogWarning("[" + gameObject.name + "] Loaded InsightServer Module: " + entry.Key.ToString());
                    }
                    if (client)
                    {
                        entry.Value.Initialize(client, this);
                        Debug.LogWarning("[" + gameObject.name + "] Loaded InsightClient Module: " + entry.Key.ToString());
                    }

                    //Add the new module to the HashSet
                    _initializedModules.Add(entry.Key);

                    // Keep checking optional if something new was initialized
                    checkOptional = true;

                    changed = true;
                }

                // If we didn't change anything, and initialized all that we could
                // with optional dependencies in mind
                if (!changed && checkOptional)
                {
                    // Initialize everything without checking optional dependencies
                    checkOptional = false;
                    continue;
                }

                // If we can no longer initialize anything
                if (!changed)
                    return !GetUninitializedModules().Any();
            }
        }

        public T GetModule<T>() where T : class, IServerModule
        {
            InsightModule module;
            _modules.TryGetValue(typeof(T), out module);

            if (module == null)
            {
                // Try to find an assignable module
                module = _modules.Values.FirstOrDefault(m => m is T);
            }

            return module as T;
        }

        public List<InsightModule> GetInitializedModules()
        {
            return _modules
                .Where(m => _initializedModules.Contains(m.Key))
                .Select(m => m.Value)
                .ToList();
        }

        public List<InsightModule> GetUninitializedModules()
        {
            return _modules
                .Where(m => !_initializedModules.Contains(m.Key))
                .Select(m => m.Value)
                .ToList();
        }
    }
}
