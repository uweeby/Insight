using Insight;
using Mirror;
using UnityEngine;

public class MasterBehaviour : MonoBehaviour
{
    public LogFilter.FilterLevel logLevel { get; set; }

    public int Port;

    public InsightServer masterServer;

    // Use this for initialization
    void Start ()
    {        
        masterServer = new InsightServer();
        masterServer.StartServer(Port);
        RegisterHandlers();
        masterServer.Connected += OnConnect;
        masterServer.Disconnected += OnDisconnect;
    }
	
	// Update is called once per frame
	void Update ()
    {
        masterServer.HandleNewMessages();
    }

    public virtual void OnConnect(Telepathy.Message msg)
    {
        print("OnConnect");
        InsightNetworkConnection conn = new InsightNetworkConnection();
        conn.Initialize(masterServer, masterServer.GetConnectionInfo(msg.connectionId), masterServer.serverHostId, msg.connectionId);
        masterServer.AddConnection(conn);
    }

    public virtual void OnDisconnect(Telepathy.Message msg)
    {
        print("OnDisconnect");
        masterServer.RemoveConnection(msg.connectionId);
    }

    private void RegisterHandlers()
    {
        masterServer.RegisterHandler(ClientToMasterTestMsg.MsgId, HandleClientToMasterTestMsg);
        masterServer.RegisterHandler(ZoneToMasterTestMsg.MsgId, HandleZoneToMasterTestMsg);
    }

    private void HandleClientToMasterTestMsg(InsightNetworkMessage netMsg)
    {
        ClientToMasterTestMsg message = netMsg.ReadMessage<ClientToMasterTestMsg>();

        print("HandleClientToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ClientToMasterTestMsg.MsgId, new ClientToMasterTestMsg() { Source = "master:5000", Desintation = "client:5000", Data = "" });
    }

    private void HandleZoneToMasterTestMsg(InsightNetworkMessage netMsg)
    {
        ZoneToMasterTestMsg message = netMsg.ReadMessage<ZoneToMasterTestMsg>();

        print("HandleZoneToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ZoneToMasterTestMsg.MsgId, new ZoneToMasterTestMsg() { Source = "master:5000", Desintation = "zone:5000", Data = "" });
    }

    private void OnApplicationQuit()
    {
        masterServer.StopServer();
    }
}
