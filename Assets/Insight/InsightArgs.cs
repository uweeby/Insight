using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public static class InsightArgs
    {
        private static Dictionary<string, List<string>> startupArgs = new Dictionary<string, List<string>>();

        private static bool _initalized;

        public static void Initialize()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string lastParam = string.Empty;

            foreach (string arg in args)
            {
                //Cache the param
                if(arg.StartsWith("-") || arg.StartsWith("--"))
                {
                    startupArgs.Add(arg, new List<string>());
                    lastParam = arg;
                    continue;
                }
                if(lastParam != string.Empty)
                {
                    startupArgs[lastParam].Add(arg);
                }
            }

            //For debug only
            foreach (KeyValuePair<string, List<string>> kvp in startupArgs)
            {
                Debug.Log("key:" + kvp.Key);

                foreach (string arg in kvp.Value)
                {
                    Debug.Log("key:" + kvp.Key + " value:" + arg);
                }
            }
        }

        public static bool TryGetArgument(string ArgName, out List<string> ArgValue)
        {
            if(!_initalized)
            {
                _initalized = true;
                Initialize();
            }

            ArgValue = new List<string>();
            if (!startupArgs.ContainsKey(ArgName)) return false;

            ArgValue.AddRange(startupArgs[ArgName]);
            return true;
        }
    }
}