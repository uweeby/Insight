using Mirror;
using UnityEngine;

public class InsightMaster : MonoBehaviour
{
    public LogFilter.FilterLevel logLevel { get; set; }

    InsightNetworkServer zoneServerConnection;
    InsightNetworkServer playerServerConnection;

    // Use this for initialization
    void Start()
    {
        zoneServerConnection = new InsightNetworkServer();
        zoneServerConnection.StartServer(5000);
        RegisterInsightZoneHandlers();
        zoneServerConnection.Connected += OnZoneConnect;
        zoneServerConnection.Disconnected += OnZoneDisconnect;

        playerServerConnection = new InsightNetworkServer();
        playerServerConnection.StartServer(7000);
        RegisterInsightPlayerHandlers();
        playerServerConnection.Connected += OnPlayerConnect;
        playerServerConnection.Disconnected += OnPlayerDisconnect;
    }

    // Update is called once per frame
    void Update()
    {
        zoneServerConnection.HandleNewMessages();
        playerServerConnection.HandleNewMessages();
    }

    public virtual void OnZoneConnect(Telepathy.Message msg)
    {
        print("OnZoneConnect");
        InsightNetworkConnection conn = new InsightNetworkConnection();
        conn.Initialize(zoneServerConnection, zoneServerConnection.GetConnectionInfo(msg.connectionId), zoneServerConnection.serverHostId, msg.connectionId);
        zoneServerConnection.AddConnection(conn);
    }

    public virtual void OnZoneDisconnect(Telepathy.Message msg)
    {
        print("OnZoneDisconnect");
        zoneServerConnection.RemoveConnection(msg.connectionId);
    }

    public virtual void OnPlayerConnect(Telepathy.Message msg)
    {
        print("OnPlayerConnect");
        InsightNetworkConnection conn = new InsightNetworkConnection();
        conn.Initialize(playerServerConnection, playerServerConnection.GetConnectionInfo(msg.connectionId), playerServerConnection.serverHostId, msg.connectionId);
        playerServerConnection.AddConnection(conn);
    }

    public virtual void OnPlayerDisconnect(Telepathy.Message msg)
    {
        print("OnPlayerDisconnect");
        playerServerConnection.RemoveConnection(msg.connectionId);
    }

    private void RegisterInsightZoneHandlers()
    {
        zoneServerConnection.RegisterHandler(ZoneToMasterTestMsg.MsgId, HandleZoneToMasterTestMsg);
    }

    private void RegisterInsightPlayerHandlers()
    {
        playerServerConnection.RegisterHandler(ClientToMasterTestMsg.MsgId, HandleClientToMasterTestMsg);
    }

    private void HandleZoneToMasterTestMsg(InsightNetworkMessage netMsg)
    {
        ZoneToMasterTestMsg message = netMsg.ReadMessage<ZoneToMasterTestMsg>();

        print("HandleZoneToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ZoneToMasterTestMsg.MsgId, new ZoneToMasterTestMsg() { Source = "master:5000", Desintation = "zone:5000", Data = "" });
    }

    private void HandleClientToMasterTestMsg(InsightNetworkMessage netMsg)
    {
        ClientToMasterTestMsg message = netMsg.ReadMessage<ClientToMasterTestMsg>();

        print("HandleClientToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ClientToMasterTestMsg.MsgId, new ClientToMasterTestMsg() { Source = "master:7000", Desintation = "client:7000", Data = "" });
    }

    private void OnApplicationQuit()
    {
        zoneServerConnection.StopServer();
        playerServerConnection.StopServer();
    }
}