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
        ChangeServerMsg message = netMsg.ReadMessage<ChangeServerMsg>();

        if (client.logNetworkMessages) { Debug.Log("[InsightClient] - Connecting to GameServer: " + message.NetworkAddress + ":" + message.NetworkPort + "/" + message.SceneName); }

        networkManager.networkAddress = message.NetworkAddress;
        transport.port = message.NetworkPort;
        SceneManager.LoadScene(message.SceneName);
        networkManager.StartClient();
    }

    public void SendStartMatchMaking()
    {
        client.Send((short)MsgId.StartMatchMaking, new StartMatchMakingMsg() { PlayListName = "SuperAwesomeGame"});
    }

    public void SendStopMatchMaking()
    {
        client.Send((short)MsgId.StopMatchMaking, new StopMatchMakingMsg());
    }

    public void SendJoinGameMsg(string UniqueID)
    {
        client.Send((short)MsgId.JoinGame, new JoinGamMsg());
    }

    public void SendGetGameListMsg()
    {
        client.Send((short)MsgId.GameList, new GameListMsg(), (callbackStatus, reader) =>
        {
            if (callbackStatus == CallbackStatus.Ok)
            {
                StatusMsg msg = reader.ReadMessage<StatusMsg>();
                Debug.Log(msg.Text);
            }
            if (callbackStatus == CallbackStatus.Error)
            {
                Debug.LogError("Callback Error: Login error");
            }
            if (callbackStatus == CallbackStatus.Timeout)
            {
                Debug.LogError("Callback Error: Login attempt timed out");
            }
        });
    }
}
