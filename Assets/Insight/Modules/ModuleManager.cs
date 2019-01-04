using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Insight
{
    [RequireComponent(typeof(InsightCommon))]
    public class ModuleManager : MonoBehaviour
    {
        InsightClient client; //Reference to the Insight objec that will be used with this module
        InsightServer server;

        public bool SearchChildrenForModule = true;

        private Dictionary<Type, InsightModule> _modules;
        private HashSet<Type> _initializedModules;

        private bool _initializeComplete;
        private bool _cachedClientAutoStartValue;
        private bool _cachedServerAutoStartValue;

        void Awake()
        {
            client = GetComponent<InsightClient>();
            server = GetComponent<InsightServer>();

            if(client)
            {
                _cachedClientAutoStartValue = client.AutoStart;
                client.AutoStart = false; //Wait until modules are loaded to AutoStart
            }

            if (server)
            {
                _cachedServerAutoStartValue = server.AutoStart;
                server.AutoStart = false; //Wait until modules are loaded to AutoStart
            }
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

                var modules = SearchChildrenForModule ? GetComponentsInChildren<InsightModule>() : FindObjectsOfType<InsightModule>();

                // Add modules
                foreach (var module in modules)
                    AddModule(module);

                // Initialize modules
                InitializeModules(client, server);


                //Register Handlers
                foreach (var module in modules)
                    module.RegisterHandlers();

                //Now that modules are loaded check for original AutoStart value
                if(_cachedServerAutoStartValue)
                {
                    server.AutoStart = _cachedServerAutoStartValue;
                    server.StartInsight();
                }

                if (_cachedClientAutoStartValue)
                {
                    client.AutoStart = _cachedClientAutoStartValue;
                    client.StartInsight();
                }
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

        public bool InitializeModules(InsightClient client, InsightServer server)
        {
            var checkOptional = true;

            // Initialize modules
            while (true)
            {
                var changed = false;
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
                    //entry.Value.Server = this;
                    if (client)
                    {
                        entry.Value.Initialize(client, this);
                        Debug.LogWarning("[" + gameObject.name + "] Loaded InsightClient Module: " + entry.Key.ToString());
                    }
                    if (server)
                    {
                        entry.Value.Initialize(server, this);
                        Debug.LogWarning("[" + gameObject.name + "] Loaded InsightServer Module: " + entry.Key.ToString());
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

        public List<InsightModule> GetUninitializedModules()
        {
            return _modules
                .Where(m => !_initializedModules.Contains(m.Key))
                .Select(m => m.Value)
                .ToList();
        }
    }
}