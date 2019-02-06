﻿using Insight;
using Mirror;
using UnityEngine;

public class ClientSpawnRequest : InsightModule
{
    InsightClient client;
    TelepathyTransport transport;

    public string GameSceneName;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        transport = client.GetComponent<TelepathyTransport>();
        transport.OnClientConnected.AddListener(ClientOnConnectedEventHandler);

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler((short)MsgId.RequestSpawn, SpawnRequestHandler);
    }

    //The spawn is requested once the Player connects to the server for simplicity.
    private void ClientOnConnectedEventHandler()
    {
        Debug.Log("[ClientSpawnRequest] - Player requesting a new game spawn");
        
        client.Send((short)MsgId.RequestSpawn, new RequestSpawn() { SceneName = GameSceneName }, (callbackStatus, reader) =>
        {
            if (callbackStatus == CallbackStatus.Ok)
            {
                Debug.Log("[ClientSpawnRequest] - Spawn request sucessful for game: '" + GameSceneName + "'", this);
                return;
            }
            if(callbackStatus == CallbackStatus.Timeout)
            {
                Debug.LogError("[ClientSpawnRequest] - Request Timed Out,");
                return;
            }
            if(callbackStatus == CallbackStatus.Error)
            {
                Debug.LogError("[ClientSpawnRequest] - Failed to spawn game: '" + GameSceneName + "'.", this);
                return;
            }
        });
    }

    private void SpawnRequestHandler(InsightNetworkMessage netMsg)
    {
        RequestSpawn message = netMsg.ReadMessage<RequestSpawn>();

        //At this point you can pass the login connection info to the NetworkManager and Start it
        //networkManager.networkAddress = message.NetworkAddress;
        //transport.port = message.NetworkPort;
        //networkManager.StartClient();

        //To confirm it worked via the console print things:
        Debug.Log("[ClientSpawnRequest] - " + message.SceneName + " was just spawned at: " + message.NetworkAddress);
    }
}
