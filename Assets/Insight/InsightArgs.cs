using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public static class InsightArgs
    {
        public static Dictionary<string, string> dictionary = new Dictionary<string, string>();

        static InsightArgs()
        {
            BuildDictionaryFromArgs();

            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                Debug.Log("key:" + kvp.Key + " value:" + kvp.Value);
            }
        }

        public static void BuildDictionaryFromArgs()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-")) //Argument Names start with a dash. Example: -NetworkAddress
                {
                    if (!args[i + 1].StartsWith("-")) //Check to make sure the next line does NOT start with a dash meaning this argument has a key and a value
                    {
                        dictionary.Add(args[i], args[i + 1]); //Add the key from this line and the value we peeked from the next line
                        i++; //skip the next line since we know it was a value used in the dictionary.Add above
                    }
                    else //The argument is just a key with no value
                    {
                        dictionary.Add(args[i], string.Empty);
                    }
                }
            }
        }

        public static string GetStringFromDictionary()
        {
            return null;
        }
    }
}