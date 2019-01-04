using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomingCmdlineArgs {

    public Dictionary<string, List<string>> commandLineArguments = new Dictionary<string, List<string>>(System.StringComparer.OrdinalIgnoreCase);

    public bool initialized { get; protected set; }

    public IncomingCmdlineArgs(params string[] prefixes)
    {
        // sort longest prefix first, so we don't get issues with '-' being found in '--' prefix. 
        prefixes = prefixes.OrderByDescending((arg) => arg.Length).ToArray();

        var args = System.Environment.GetCommandLineArgs().ToList();

        int index = 1; // first item in the command line list is the name of the app itself. 
        string token = "";
        string currentCommand = "";

        while(index < args.Count)
        {
            token = args[index];

            Debug.Log("Checking token '" + token + "'");

            foreach (var prefix in prefixes)
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

        initialized = true;
    }


    public bool TryGetArgument(string command, out List<string> arguments)
    {
        if(!initialized)
        {
            throw new System.InvalidOperationException("Object has not been Initialized.");
        }

        arguments = new List<string>();
        if (!commandLineArguments.ContainsKey(command)) return false;

        arguments.AddRange(commandLineArguments[command]);
        return true;
    }

}
