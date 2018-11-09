using Insight;
using Mirror;
using UnityEngine;

public class MasterClientBehaviour : MonoBehaviour
{
    public LogFilter.FilterLevel logLevel { get; set; }

    public int Port;

    InsightServer clientServer;

    // Use this for initialization
    void Start ()
    {
        clientServer = new InsightServer();
        clientServer.StartServer(Port);
        RegisterInsightPlayerHandlers();
        clientServer.Connected += OnPlayerConnect;
        clientServer.Disconnected += OnPlayerDisconnect;
    }
	
	// Update is called once per frame
	void Update ()
    {
        clientServer.HandleNewMessages();
    }

    public virtual void OnPlayerConnect(Telepathy.Message msg)
    {
        print("OnPlayerConnect");
        InsightNetworkConnection conn = new InsightNetworkConnection();
        conn.Initialize(clientServer, clientServer.GetConnectionInfo(msg.connectionId), clientServer.serverHostId, msg.connectionId);
        clientServer.AddConnection(conn);
    }

    public virtual void OnPlayerDisconnect(Telepathy.Message msg)
    {
        print("OnPlayerDisconnect");
        clientServer.RemoveConnection(msg.connectionId);
    }

    private void RegisterInsightPlayerHandlers()
    {
        clientServer.RegisterHandler(ClientToMasterTestMsg.MsgId, HandleClientToMasterTestMsg);
    }

    private void HandleClientToMasterTestMsg(InsightNetworkMessage netMsg)
    {
        ClientToMasterTestMsg message = netMsg.ReadMessage<ClientToMasterTestMsg>();

        print("HandleClientToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ClientToMasterTestMsg.MsgId, new ClientToMasterTestMsg() { Source = "master:7000", Desintation = "client:7000", Data = "" });
    }

    private void OnApplicationQuit()
    {
        clientServer.StopServer();
    }
}
