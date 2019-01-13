﻿using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public static class InsightArgs
    {
        private static Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();

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
                    dictionary.Add(arg, new List<string>());
                    lastParam = arg;
                    continue;
                }
                if(lastParam != string.Empty)
                {
                    dictionary[lastParam].Add(arg);
                }
            }

            //For debug only
            foreach (KeyValuePair<string, List<string>> kvp in dictionary)
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
                Initialize();
            }

            ArgValue = new List<string>();
            if (!dictionary.ContainsKey(ArgName)) return false;

            ArgValue.AddRange(dictionary[ArgName]);
            return true;
        }
    }
}