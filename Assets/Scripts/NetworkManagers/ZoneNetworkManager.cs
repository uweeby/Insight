using Mirror;

public class ZoneNetworkManager : NetworkManager
{

	// Use this for initialization
	void Start ()
    {
        networkPort = 7777;
        StartServer();
        RegisterHandlers();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void RegisterHandlers()
    {
        NetworkServer.RegisterHandler(ClientToZoneTestMsg.MsgId, HandleClientToZoneTestMsg);
    }

    private void HandleClientToZoneTestMsg(NetworkMessage netMsg)
    {
        ClientToZoneTestMsg message = netMsg.ReadMessage<ClientToZoneTestMsg>();

        print("HandleClientToZoneTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
        netMsg.conn.Send(ClientToZoneTestMsg.MsgId, new ClientToZoneTestMsg() { Source = "zone:7777", Desintation = "client:7777", Data = "" });
    }
}
