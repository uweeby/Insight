using Mirror;
using UnityEngine;

public class InsightMaster : MonoBehaviour
{
    public LogFilter.FilterLevel logLevel { get; set; }

    //Zone Server Connection:
    InsightNetworkServer zoneServerConnection;
    //public List<ZoneContainer> Zones;
    //public string ZoneServerAuthCode;

    //Player Server Connection:
    InsightNetworkServer playerServerConnection;
    //public List<PlayerContainer> Players;

    //public InsightSpawner spawner;

    // Use this for initialization
    void Start()
    {
        zoneServerConnection = new InsightNetworkServer();
        RegisterInsightZoneHandlers();
        zoneServerConnection.StartServer(5000);
        zoneServerConnection.Connected += OnZoneConnect;
        zoneServerConnection.Disconnected += OnZoneDisconnect;

        playerServerConnection = new InsightNetworkServer();
        RegisterInsightPlayerHandlers();
        playerServerConnection.StartServer(7000);
    }

    // Update is called once per frame
    void Update()
    {
        zoneServerConnection.HandleNewMessages();
        //playerServerConnection.HandleNewMessages();

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    spawner.SpawnStaticZones();
        //}
    }

    public virtual void OnZoneConnect(Telepathy.Message msg)
    {
        print("OnZoneConnect");
        //string address = zoneServerConnection.GetConnectionInfo(msg.connectionId);
        //Debug.Log("ID: " + msg.connectionId + " Connected from: " + address);
    }

    public virtual void OnZoneDisconnect(Telepathy.Message msg)
    {
        print("OnZoneDisconnect");
        //string address = zoneServerConnection.GetConnectionInfo(msg.connectionId); //ConnectionID already gone at this point
        //Debug.Log("ID: " + msg.connectionId + " Disconnected");
    }

    private void RegisterInsightZoneHandlers()
    {
        //zoneServerConnection.RegisterHandler(InsightAuthCode.MsgId, HandleZoneAuthCodeMsg);
        zoneServerConnection.RegisterHandler((short)MsgType.Connect, AltConnect);
        zoneServerConnection.RegisterHandler((short)MsgType.Disconnect, AltDisconnect);
    }

    private void RegisterInsightPlayerHandlers()
    {
        //zoneServerConnection.RegisterHandler(InsightAuthCode.MsgId, HandleZoneAuthCodeMsg);
    }

    private void AltConnect(NetworkMessage netmsg)
    {
        print("AltConnect");
    }

    private void AltDisconnect(NetworkMessage netmsg)
    {
        print("AltDisconnect");
    }

    //private void HandleZoneAuthCodeMsg(NetworkMessage netMsg)
    //{
    //    Debug.Log("HandleAuthCodeMsg");
    //    InsightAuthCode message = netMsg.ReadMessage<InsightAuthCode>();

    //    if (ZoneServerAuthCode.Equals(message.AuthCode))
    //    {
    //        //Zones.Add(new ZoneContainer() { Address = netMsg.})
    //    }
    //}

    private void OnApplicationQuit()
    {
        zoneServerConnection.StopServer();
        //playerServerConnection.StopServer();
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