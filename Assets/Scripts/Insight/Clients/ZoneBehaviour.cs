using UnityEngine;
using Insight;

public class ZoneBehaviour : MonoBehaviour
{
    InsightClient insight;

	// Use this for initialization
	void Start ()
    {
        DontDestroyOnLoad(gameObject);

        insight = new InsightClient();
        RegisterHandlers();
        insight.StartClient("localhost", 5000);
    }
	
	// Update is called once per frame
	void Update ()
    {
        insight.HandleNewMessages();

        //Msg to Master
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            insight.SendMsg(ZoneToMasterTestMsg.MsgId, new ZoneToMasterTestMsg() { Source = "zone:5000", Desintation = "master:5000", Data = "cvbdfgert" });
        }
    }

    private void RegisterHandlers()
    {
        insight.RegisterHandler(ZoneToMasterTestMsg.MsgId, HandleZoneToMasterTestMsg);
    }

    private void HandleZoneToMasterTestMsg(InsightNetworkMessage netMsg)
    {
        ZoneToMasterTestMsg message = netMsg.ReadMessage<ZoneToMasterTestMsg>();

        print("HandleZoneToMasterTestMsg - Source: " + message.Source + " Destination: " + message.Desintation);
    }

    private void OnApplicationQuit()
    {
        insight.StopClient();
    }
}
