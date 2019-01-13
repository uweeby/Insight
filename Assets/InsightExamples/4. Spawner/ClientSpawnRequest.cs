using Insight;
using UnityEngine;

public class ClientSpawnRequest : InsightModule
{
    InsightClient client;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        client.OnConnectedEvent.AddListener(ClientOnConnectedEventHandler);

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler(SpawnRequest.MsgId, SpawnRequestHandler);
    }

    private void ClientOnConnectedEventHandler()
    {
        //The spawn is requested once the Player connects to the server for simplicity.
        //Normally this would be called via a GUI or something
        string ExampleGameName = "SuperAwesomeGame"; //This would probably get passed in

        client.Send(SpawnRequest.MsgId, new SpawnRequest() { GameName = ExampleGameName }, (status, reader) =>
        {
            if (status == CallbackStatus.Ok)
            {
                // excellent, we are registered! 
                Debug.Log("Callback: spawn request sucessful for game: '" + ExampleGameName + "'", this);
            }
            else
            {
                // bummer, we should try to re-register or throw an error or something. 
                Debug.LogError("Callback: failed to spawn game: '" + ExampleGameName + "'.", this);
                return;
            }
        });
    }

    private void SpawnRequestHandler(InsightNetworkMessage netMsg)
    {
        SpawnRequest message = netMsg.ReadMessage<SpawnRequest>();

        //The new server that was spawned should probably be running a NetworkManager.
        //So you could set the players NetworkManager to use the NetworkAddress and NetworkPort from this msg

        //networkManager.NetworkAddress = message.NetworkAddress
        //networkManager.NetworkPort = message.NetworkPort

        //To confirm it worked via the console print things:
        Debug.Log(message.GameName + " was just spawned at: " + message.NetworkAddress + ":" + message.NetworkPort);
    }
}