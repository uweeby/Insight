using Insight;
using Mirror;
using UnityEngine;

public class ClientSpawnRequest : InsightModule
{
    InsightClient client;

    public NetworkManager networkManager;
    public TelepathyTransport transport;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        client.OnConnectedEvent.AddListener(ClientOnConnectedEventHandler);

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler((short)MsgId.RequestSpawn, SpawnRequestHandler);
    }

    private void ClientOnConnectedEventHandler()
    {
        //The spawn is requested once the Player connects to the server for simplicity.
        //Normally this would be called via a GUI or something
        string ExampleGameName = "SuperAwesomeGame"; //This would probably get passed in

        client.Send((short)MsgId.RequestSpawn, new RequestSpawn() { GameName = ExampleGameName }, (status, reader) =>
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
        RequestSpawn message = netMsg.ReadMessage<RequestSpawn>();

        networkManager.networkAddress = message.NetworkAddress;
        transport.port = message.NetworkPort;

        //To confirm it worked via the console print things:
        Debug.Log(message.GameName + " was just spawned at: " + message.NetworkAddress + ":" + message.NetworkPort);
    }
}
