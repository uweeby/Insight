using Insight;
using Telepathy;
using UnityEngine;

[RequireComponent(typeof(ZoneNetworkManager))]
public class ZoneInsightClient : InsightClient
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
}
