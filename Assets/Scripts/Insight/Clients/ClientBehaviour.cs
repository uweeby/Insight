using Insight;
using UnityEngine;

public class ClientBehaviour : MonoBehaviour
{
    InsightClient insight;

    public int Port;

	// Use this for initialization
	void Start ()
    {
        DontDestroyOnLoad(gameObject);

        insight = new InsightClient();
        RegisterHandlers();
        insight.StartClient("localhost", Port);
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

        //Msg to Master
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            insight.SendMsg(ChatMessage.MsgId, new ChatMessage() { Message = "/t player testing" });
            insight.SendMsg(ChatMessage.MsgId, new ChatMessage() { Message = "/s local chat here" });
            insight.SendMsg(ChatMessage.MsgId, new ChatMessage() { Message = "/y global chat here" });
            insight.SendMsg(ChatMessage.MsgId, new ChatMessage() { Message = "/g guild chat here" });
            insight.SendMsg(ChatMessage.MsgId, new ChatMessage() { Message = "/p party chat here" });
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
