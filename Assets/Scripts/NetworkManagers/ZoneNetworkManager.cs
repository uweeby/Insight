using Insight;
using Mirror;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZoneNetworkManager : NetworkManager
{
    public int maxPlayers;
    public int currentPlayers;

    // Use this for initialization
    void Start ()
    {
        networkPort = 7777;
        StartServer();
        RegisterHandlers();

        ParseArgs();
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

    public void ParseArgs()
    {
        InsightArgs args = new InsightArgs();
        if(args.IsProvided("-ScenePath"))
        {
            Debug.Log("ScenePath: " + args.ExtractValue("-ScenePath"));
            SceneManager.LoadScene(args.ExtractValue("-ScenePath"));
        }
    }
}
