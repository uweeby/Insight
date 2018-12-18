using Insight;
using UnityEngine;

[RequireComponent(typeof(ClientNetworkManager))]
public class PlayerInsightClient : InsightClient
{
    public ClientNetworkManager networkManager;

    public string AuthID;

    // Use this for initialization
    public override void Start ()
    {
        base.Start();

        networkManager = GetComponent<ClientNetworkManager>();

        RegisterHandlers();

        StartClient(networkAddress, networkPort);
    }
	
	// Update is called once per frame
	public override void Update ()
    {
        //Msg to Master
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Private, Origin = "", Target = "", Message = "private chat test msg" });
            SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Public, Origin = "", Message = "public chat test msg" });
            SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Global, Origin = "", Message = "global chat test msg" });
            SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Party, Origin = "", Message = "party chat test msg" });
            SendMsg(InsightChatMessage.MsgId, new InsightChatMessage() { ChannelType = (short)ChatChannelType.Guild, Origin = "", Message = "guild chat test msg" });
        }
    }

    void RegisterHandlers()
    {
        
    }

    private void HandleClientToMasterTestMsg(InsightNetworkMessage netMsg)
    {

    }

    private void OnApplicationQuit()
    {
        StopClient();
    }
}
