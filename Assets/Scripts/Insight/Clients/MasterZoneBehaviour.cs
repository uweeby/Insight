using Insight;
using Mirror;
using UnityEngine;

public class MasterZoneBehaviour : MonoBehaviour
{
    public LogFilter.FilterLevel logLevel { get; set; }

    public int Port;

    InsightServer zoneServer;

    // Use this for initialization
    void Start ()
    {
        zoneServer = new InsightServer();
        zoneServer.StartServer(Port);
        RegisterInsightZoneHandlers();
        zoneServer.Connected += OnZoneConnect;
        zoneServer.Disconnected += OnZoneDisconnect;
    }
	
	// Update is called once per frame
	void Update ()
    {
        zoneServer.HandleNewMessages();
    }

    public virtual void OnZoneConnect(Telepathy.Message msg)
    {
        print("OnZoneConnect");
        InsightNetworkConnection conn = new InsightNetworkConnection();
        conn.Initialize(zoneServer, zoneServer.GetConnectionInfo(msg.connectionId), zoneServer.serverHostId, msg.connectionId);
        zoneServer.AddConnection(conn);
    }

    public virtual void OnZoneDisconnect(Telepathy.Message msg)
    {
        print("OnZoneDisconnect");
        zoneServer.RemoveConnection(msg.connectionId);
    }

    private void RegisterInsightZoneHandlers()
    {
        zoneServer.RegisterHandler(ZoneToMasterTestMsg.MsgId, HandleZoneToMasterTestMsg);
    }

    private void HandleZoneToMasterTestMsg(InsightNetworkMessage netMsg)
    {
        ZoneToMasterTestMsg message = netMsg.ReadMessage<ZoneToMasterTestMsg>();

        print("HandleZoneToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ZoneToMasterTestMsg.MsgId, new ZoneToMasterTestMsg() { Source = "master:5000", Desintation = "zone:5000", Data = "" });
    }

    private void OnApplicationQuit()
    {
        zoneServer.StopServer();
    }
}
