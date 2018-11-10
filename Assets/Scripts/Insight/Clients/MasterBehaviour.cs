using Insight;

public class MasterBehaviour : InsightServer
{
    // Use this for initialization
    void Start ()
    {        
        StartServer(networkPort);
        RegisterHandlers();
    }
	
	// Update is called once per frame
	void Update ()
    {
        HandleNewMessages();
    }

    public override void OnConnect(Telepathy.Message msg)
    {
        print("OnConnect");
        InsightNetworkConnection conn = new InsightNetworkConnection();
        conn.Initialize(this, GetConnectionInfo(msg.connectionId), serverHostId, msg.connectionId);
        AddConnection(conn);
    }

    public override void OnDisconnect(Telepathy.Message msg)
    {
        print("OnDisconnect");
        RemoveConnection(msg.connectionId);
    }

    public override void OnServerStart()
    {

    }

    public override void OnServerStop()
    {

    }

    private void RegisterHandlers()
    {
        RegisterHandler(ClientToMasterTestMsg.MsgId, HandleClientToMasterTestMsg);
        RegisterHandler(ZoneToMasterTestMsg.MsgId, HandleZoneToMasterTestMsg);
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
        StopServer();
    }
}
