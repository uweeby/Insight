using Insight;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameRegistration : InsightModule
{
    InsightClient client;
    public NetworkManager networkManager;
    public TelepathyTransport telepathyTransport;

    public List<GameContainer> registeredGames = new List<GameContainer>();

    public override void Initialize(InsightClient insight, ModuleManager manager)
    {
        client = insight;

        client.OnConnectedEvent.AddListener(ClientOnConnectedEventHandler);

        RegisterHandlers();

        networkManager = NetworkManager.singleton;
    }

    void RegisterHandlers()
    {
    }

    private void ClientOnConnectedEventHandler()
    {
        //Gather Params
        List<string> portArgs;
        if(InsightArgs.TryGetArgument("-AssignedPort", out portArgs))
        {
            Debug.Log("Setting Network Port based on Args provided: " + portArgs[0]);
            telepathyTransport.port = Convert.ToUInt16(portArgs[0]);
        }

        //Apply necessary changes to NetworkManager

        //Start NetworkManager
        networkManager.StartServer();

        //Register back to the GameManager now that the game is running
        SendGameRegistrationToGameManager();
    }

    private void SendGameRegistrationToGameManager()
    {
        Debug.Log("sending registration msg back to master");
        client.Send((short)MsgId.RegisterGame, new RegisterGameMsg() { UniqueID = Guid.NewGuid().ToString()}); //The GUID can/should be provided by the spawner for security
    }
}
