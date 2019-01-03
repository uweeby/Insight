using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomingCmdlineArgs : MonoBehaviour {

    public List<string> argumentPrefixes;

    public Dictionary<string, List<string>> commandLineArguments = new Dictionary<string, List<string>>(System.StringComparer.OrdinalIgnoreCase);

	// Use this for initialization
	void Start () {

        // sort longest prefix first, so we don't get issues with '-' being found in '--' prefix. 
        argumentPrefixes = argumentPrefixes.OrderByDescending((arg) => arg.Length).ToList();

        var args = System.Environment.GetCommandLineArgs().ToList();

        int index = 1; // first item in the command line list is the name of the app itself. 
        string token = "";
        string currentCommand = "";

        while(index < args.Count)
        {
            token = args[index];

            Debug.Log("Checking token '" + token + "'", this);

            foreach (var prefix in argumentPrefixes)
            {
                if (token.Contains(prefix))
                {
                    // is a command. 
                    currentCommand = token.Substring(prefix.Length);
                    commandLineArguments[currentCommand] = new List<string>();
                    index++;
                    continue;
                }
            }

            // if we get here, the token is _not_ a command, but a pram to the `currentCommand` item. 
            commandLineArguments[currentCommand].Add(token);
            index++;
        }
    }


    public bool TryGetArgument(string command, out List<string> arguments)
    {
        arguments = new List<string>();
        if (!commandLineArguments.ContainsKey(command)) return false;

        arguments.AddRange(commandLineArguments[command]);
        return true;
    }

}
