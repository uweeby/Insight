using Insight;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientMatchMaking : InsightModule
{
    InsightClient client;
    public NetworkManager networkManager;
    public TelepathyTransport transport;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler((short)MsgId.ChangeServers, HandleChangeServersMsg);
    }

    private void HandleChangeServersMsg(InsightNetworkMessage netMsg)
    {
        ChangeServers message = netMsg.ReadMessage<ChangeServers>();

        if (client.logNetworkMessages) { Debug.Log("[InsightClient] - Connecting to GameServer: " + message.NetworkAddress + ":" + message.NetworkPort + "/" + message.SceneName); }

        networkManager.networkAddress = message.NetworkAddress;
        transport.port = message.NetworkPort;
        SceneManager.LoadScene(message.SceneName);
        networkManager.StartClient();
    }

    public void SendStartMatchMaking()
    {
        client.Send((short)MsgId.StartMatchMaking, new StartMatchMaking() { PlayListName = "SuperAwesomeGame"});
    }

    public void SendStopMatchMaking()
    {
        client.Send((short)MsgId.StopMatchMaking, new StopMatchMaking());
    }
}
