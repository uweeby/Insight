using Insight;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSpawnRequest : InsightModule
{
    InsightClient client;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        client.OnConnectedEvent.AddListener(ClientOnConnectedEventHandler);
    }

    public override void RegisterHandlers()
    {
        client.RegisterHandler(SpawnDataMessage.MsgId, SpawnDataMessageHandler);
    }

    private void ClientOnConnectedEventHandler()
    {
        //The spawn is requested once the Player connects to the server for simplicity.
        //Normally this would be called via a GUI or something
        string ExampleGameName = "SuperAwesomeGame"; //This would probably get passed in

        client.Send(SpawnDataMessage.MsgId, new SpawnDataMessage() { GameName = ExampleGameName }, (status, reader) =>
        {
            if (status == CallbackStatus.Ok)
            {
                // excellent, we are registered! 
                Debug.Log("Registered for key '" + ExampleGameName + "'", this);
            }
            else
            {
                // bummer, we should try to re-register or throw an error or something. 
                Debug.LogError("Unable to register spawner key '" + ExampleGameName + "'.", this);
                return;
            }
        });
    }

    private void SpawnDataMessageHandler(InsightNetworkMessage netMsg)
    {
        SpawnDataMessage message = netMsg.ReadMessage<SpawnDataMessage>();

        //The new server that was spawned should probably be running a NetworkManager.
        //So you could set the players NetworkManager to use the NetworkAddress and NetworkPort from this msg

        //networkManager.NetworkAddress = message.NetworkAddress
        //networkManager.NetworkPort = message.NetworkPort

        //To confirm it worked via the console print things:
        Debug.Log(message.GameName + " was just spawned at: " + message.NetworkAddress + ":" + message.NetworkPort);
    }
}