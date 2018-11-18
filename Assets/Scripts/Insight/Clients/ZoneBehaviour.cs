using UnityEngine;
using Insight;
using Telepathy;

[RequireComponent(typeof(ZoneNetworkManager))]
public class ZoneBehaviour : InsightClient
{
    public ZoneNetworkManager networkManager;

    public ZoneContainer container;

    private InsightArgs insightArguments = new InsightArgs();
    

    // Use this for initialization
    public override void Start ()
    {
        base.Start();

        networkManager = GetComponent<ZoneNetworkManager>();

        RegisterHandlers();

        CacheArgs();

        if (insightArguments.IsProvided("-MasterIp") && insightArguments.IsProvided("-MasterPort"))
        {
            Debug.Log("Master Server Args: " + insightArguments.ExtractValue("-MasterIp") + ":" + insightArguments.ExtractValueInt("-MasterPort"));
            StartClient(insightArguments.ExtractValue("-MasterIp"), insightArguments.ExtractValueInt("-MasterPort"));
        }
        else
        {
            Debug.Log("Master Server Args Not Provided. Assuming Defaults: localhost:5000");
            StartClient("localhost", 5000);
        }
    }
	
    private void RegisterHandlers()
    {
        RegisterHandler(RegisterServerConnectionMsg.MsgId, HandleRegisterServerConnectionMsgReply);
    }

    public override void OnClientStart()
    {
        print("OnClientStart");

        base.OnClientStart();
    }

    public override void OnClientStop()
    {
        print("OnClientStop");

        base.OnClientStop();
    }

    public override void OnConnected(Message msg)
    {
        print("OnConnected");

        base.OnConnected(msg);

        if (!insightArguments.IsProvided("-UniqueID"))
        {
            Debug.LogError("Not a spawned Instanced.");
            return;
        }

        //SendMsg(RegisterServerConnectionMsg.MsgId, new RegisterServerConnectionMsg() { UniqueID = container.UniqueID });
    }

    private void CacheArgs()
    {
        container = new ZoneContainer();
        container.UniqueID = insightArguments.ExtractValue("-UniqueID");
        container.ScenePath = insightArguments.ExtractValue("-ScenePath");
        container.NetworkAddress = networkManager.networkAddress;
        container.NetworkPort = networkManager.networkPort;
        container.MaxPlayers = networkManager.maxPlayers;
        container.CurentPlayers = networkManager.currentPlayers;
    }

    private void HandleRegisterServerConnectionMsgReply(InsightNetworkMessage netMsg)
    {
        print("HandleRegisterServerConnectionMsgReply");

        SendMsg(RegisterZoneMsg.MsgId, new RegisterZoneMsg()
        {
            UniqueID = container.UniqueID,
            ScenePath = container.ScenePath,
            NetworkAddress = container.NetworkAddress,
            NetworkPort = container.NetworkPort,
            MaxPlayers = container.MaxPlayers,
        });
    }
}
