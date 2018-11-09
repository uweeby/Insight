using Mirror;
using UnityEngine;

public class InsightClient : MonoBehaviour
{
    InsightNetworkClient insight;

	// Use this for initialization
	void Start ()
    {
        DontDestroyOnLoad(gameObject);

        insight = new InsightNetworkClient();
        RegisterHandlers();
        insight.StartClient("localhost", 7000);
    }
	
	// Update is called once per frame
	void Update ()
    {
        insight.HandleNewMessages();

        //Msg to Master
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            insight.SendMsg(ClientToMasterTestMsg.MsgId, new ClientToMasterTestMsg() { Source = "client:7000", Desintation = "master:7000", Data = "sdfwer234" });
        }
    }

    void RegisterHandlers()
    {
        insight.RegisterHandler(ClientToMasterTestMsg.MsgId, HandleClientToMasterTestMsg);
    }

    private void HandleClientToMasterTestMsg(InsightNetworkMessage netMsg)
    {
        ClientToMasterTestMsg message = netMsg.ReadMessage<ClientToMasterTestMsg>();

        print("HandleClientToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
    }

    private void OnApplicationQuit()
    {
        insight.StopClient();
    }
}
