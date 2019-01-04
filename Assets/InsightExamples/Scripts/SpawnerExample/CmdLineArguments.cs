using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I'm assuming here that all command line argument are going to be in the general 
// format of:
// '-commandA paramA1 paramA2 -commandB paramB1 -commandC -commandD'
//
// We strip the prefix from all commands, and make them all case-insensitive for the
// TryGetCommand method.
public class CmdLineArguments 
{
    private Dictionary<string, List<string>> commands = new Dictionary<string, List<string>>(System.StringComparer.OrdinalIgnoreCase);

    public bool initialized { get; protected set; }

    public CmdLineArguments(params string[] prefixes)
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
                    commands[currentCommand] = new List<string>();
                    index++;
                    continue;
                }
            }

            // if we get here, the token is _not_ a command, but a pram to the `currentCommand` item. 
            commands[currentCommand].Add(token);
            index++;
        }

        initialized = true;
    }


    public bool TryGetCommand(string command, out List<string> arguments)
    {
        if(!initialized)
        {
            throw new System.InvalidOperationException("Object has not been Initialized.");
        }

        arguments = new List<string>();
        if (!commands.ContainsKey(command)) return false;

        arguments.AddRange(commands[command]);
        return true;
    }

}
