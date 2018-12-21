using Insight;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(InsightCommon))]
public class ModuleManager : MonoBehaviour
{
    InsightCommon insightServer;

    public bool SearchChildrenForModule;

    private Dictionary<Type, InsightModule> _modules;
    private HashSet<Type> _initializedModules;

    private bool _initializeComplete;
    // Use this for initialization
    void Start ()
    {
        insightServer = GetComponent<InsightCommon>();

        _modules = new Dictionary<Type, InsightModule>();
        _initializedModules = new HashSet<Type>();
    }

    void Update()
    {
        if(!_initializeComplete)
        {
            _initializeComplete = true;

            var modules = SearchChildrenForModule ? GetComponentsInChildren<InsightModule>() :
            FindObjectsOfType<InsightModule>();

            // Add modules
            foreach (var module in modules)
                AddModule(module);

            // Initialize modules
            InitializeModules(insightServer);

            //Register Handlers
            foreach (var module in modules)
                module.RegisterHandlers();

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

    public bool InitializeModules(InsightCommon server)
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
                entry.Value.Initialize(server);
                _initializedModules.Add(entry.Key);
                Debug.LogWarning("Loaded Module: " + entry.Key.ToString());

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
