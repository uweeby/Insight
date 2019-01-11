using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Insight;
using Mirror;
using System;

public class SpawnRegistrationModule : InsightModule
{
    // hard-coded for now, could be a [SerializedField] if it needs to 
    // be setable, however we will be controllig how these are sent to the
    // spawned process, so we should just make sure it is the same everywhere. 
    string[] prefixes = new string[2] { "-", "--" };

    InsightServer server;
    InsightClient client;

    CmdLineArguments cmdline;

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;
    }
    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;
        this.client.OnConnectedEvent.AddListener(OnClientConnectedEventHandler);
    }

    public override void RegisterHandlers()
    {
        if(server)
        {
            // handle calls from SPAWNED processes. 
            server.RegisterHandler(SpawnedProcessRegistrationMessage.MsgId, OnRegisterMessageHandler);
            // also needs to listen for the SPAWNER SERVERS messages about closes/exited/stopped processes
            // and remove them from its list of registered servers, if it exists on the list. 
            // that message should include the GUID of the process, which was given to/created by teh spawning module.
        }
        if(client)
        {
            // need to connect to a specific _spawn registration server_
            cmdline = new CmdLineArguments(prefixes);

            List<string> arguments;

            string serverIP = "";
            int serverPort = 0;

            if(cmdline.TryGetCommand("registrationip", out arguments))
            {
                if (arguments.Count == 1) serverIP = arguments[0];
                else Debug.LogError("incorrect # of arguments for cmdline item 'registrationip'.", this);

                if (cmdline.TryGetCommand("registractionport", out arguments))
                {
                    if (arguments.Count == 1)
                    {
                        serverPort = int.Parse(arguments[0]);
                        client.networkPort = serverPort;
                        client.networkAddress = serverIP;
                    }
                    else Debug.LogError("incorrect # of arguments for cmdline item 'registrationport'.", this);
                }
                else Debug.LogWarning("Unable to find registraction port cmdline args.", this);
            }
            else
            {
                Debug.LogWarning("Unable to find registration ip cmdline args", this);
            }
        }
    }

    #region SERVER
    private void OnRegisterMessageHandler(InsightNetworkMessage netMsg)
    {
        // do some specific things with the message
        // - add to a model which contains a list of all registered spawned items. 
        // - send an event/message that a new item has become available (?)
        // - ??
        var msg = netMsg.ReadMessage<SpawnedProcessRegistrationMessage>();

        Debug.Log("Registered spawned item '" + msg.guid + "'.", this);

        netMsg.Reply();
    }
    #endregion
    #region CLIENT

    private void OnClientConnectedEventHandler()
    {
        client.Send(SpawnedProcessRegistrationMessage.MsgId, new SpawnedProcessRegistrationMessage() { guid = "01" }, (status, reader) =>
        {
            if(status == CallbackStatus.Ok)
            {
#if UNITY_EDITOR
                Debug.Log("Registered with registration server.", this);
#endif
            }
            else
            {
                Debug.LogError("Unable to register with server...", this);
            }
        });
    }
    #endregion
}

public class SpawnedProcessRegistrationMessage : MessageBase
{
    public static short MsgId = 7777; // need a way to find the global message IDs and what has what range. 
    public string guid;
    // whatever else we want to send back. 
    // this type is probably going to be overridden for specific 
    // use cases, as is/should be this class as a whole. 
    // sadly, you have to have a different registration method for each
    // as you can't "subclass" a message id handler... 
}
