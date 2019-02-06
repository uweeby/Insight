using Insight;
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
        
        client.Send((short)MsgId.RequestSpawn, new RequestSpawn() { SceneName = GameSceneName }, (status, reader) =>
        {
            if (status == CallbackStatus.Ok)
            {
                // excellent, we are registered! 
                Debug.Log("Callback: spawn request sucessful for game: '" + GameSceneName + "'", this);
            }
            else
            {
                // bummer, we should try to re-register or throw an error or something. 
                Debug.LogError("Callback: failed to spawn game: '" + GameSceneName + "'.", this);
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
