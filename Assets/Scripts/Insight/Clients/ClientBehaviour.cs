using Insight;
using UnityEngine;

public class ClientBehaviour : MonoBehaviour
{
    InsightClient insight;

    public int NetworkPort;

	// Use this for initialization
	void Start ()
    {
        DontDestroyOnLoad(gameObject);

        insight = new InsightClient();
        RegisterHandlers();
        insight.StartClient("localhost", NetworkPort);
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
            insight.SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Private, Origin = "", Target = "", Message = "private chat test msg" });
            insight.SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Public, Origin = "", Message = "public chat test msg" });
            insight.SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Global, Origin = "", Message = "global chat test msg" });
            insight.SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Party, Origin = "", Message = "party chat test msg" });
            insight.SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Guild, Origin = "", Message = "guild chat test msg" });
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
