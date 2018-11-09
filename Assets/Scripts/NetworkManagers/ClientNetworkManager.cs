using Mirror;
using UnityEngine;

public class ClientNetworkManager : NetworkManager
{

	// Use this for initialization
	void Start ()
    {
        networkPort = 7777;
        StartClient();
        RegisterHandlers();
    }

    // Update is called once per frame
    void Update ()
    {
        //Msg to Zone
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            client.Send(ClientToZoneTestMsg.MsgId, new ClientToZoneTestMsg() { Source = "client:7777", Desintation = "zone:7777", Data = "zxcasdqwe123" });
        }
    }

    private void RegisterHandlers()
    {
        client.RegisterHandler(ClientToZoneTestMsg.MsgId, HandleClientToZoneTestMsg);
    }

    private void HandleClientToZoneTestMsg(NetworkMessage netMsg)
    {
        ClientToZoneTestMsg message = netMsg.ReadMessage<ClientToZoneTestMsg>();

        print("HandleClientToZoneTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
    }
}
