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
    }

    public virtual void OnZoneDisconnect(Telepathy.Message msg)
    {
        print("OnZoneDisconnect");
    }

    public virtual void OnPlayerConnect(Telepathy.Message msg)
    {
        print("OnPlayerConnect");
    }

    public virtual void OnPlayerDisconnect(Telepathy.Message msg)
    {
        print("OnPlayerDisconnect");
    }

    private void RegisterInsightZoneHandlers()
    {
        zoneServerConnection.RegisterHandler(ZoneToMasterTestMsg.MsgId, HandleZoneToMasterTestMsg);
    }

    private void RegisterInsightPlayerHandlers()
    {
        playerServerConnection.RegisterHandler(ClientToMasterTestMsg.MsgId, HandleClientToMasterTestMsg);
    }

    private void HandleZoneToMasterTestMsg(NetworkMessage netMsg)
    {
        ZoneToMasterTestMsg message = netMsg.ReadMessage<ZoneToMasterTestMsg>();

        print("HandleZoneToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ZoneToMasterTestMsg.MsgId, new ZoneToMasterTestMsg() { Source = "master:5000", Desintation = "zone:5000", Data = "" });
    }

    private void HandleClientToMasterTestMsg(NetworkMessage netMsg)
    {
        ClientToMasterTestMsg message = netMsg.ReadMessage<ClientToMasterTestMsg>();

        print("HandleClientToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ClientToMasterTestMsg.MsgId, new ClientToMasterTestMsg() { Source = "client:7000", Desintation = "client:7000", Data = "" });
    }

    private void OnApplicationQuit()
    {
        zoneServerConnection.StopServer();
        playerServerConnection.StopServer();
    }
}

//[Serializable]
//public struct ZoneContainer
//{
//    public string Address;
//    public int Port;
//    public string AuthCode;
//    public string HostedScene;
//    //public Dictionary<string, string> ZoneProperties;
//}

//[Serializable]
//public struct PlayerContainer
//{
//    public string Address;
//    public int Port;
//    public string AuthCode;
//    public string HostedScene;
//    //public Dictionary<string, string> ZoneProperties;
//}