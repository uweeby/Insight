using Insight;
using Mirror;
using UnityEngine;

public class ServerChatModule : InsightModule
{
    InsightCommon insight;

    public override void Initialize(InsightCommon insight)
    {
        this.insight = insight;
    }

    public override void RegisterHandlers()
    {
        insight.RegisterHandler(InsightChatMessage.MsgId, HandleChatMessage);
    }

    private void HandleChatMessage(InsightNetworkMessage netMsg)
    {
        InsightChatMessage message = netMsg.ReadMessage<InsightChatMessage>();

        switch(message.ChannelType)
        {
            //Send message from Origin to Target
            case (short)ChatChannelType.Private:
                Debug.Log("Type: " + message.ChannelType + " Origin: " + message.Origin + " Target: " + message.Target + " Message: " + message.Message);
                //NetworkConnection conn = insightServer.FindConnectionByPlayer(message.Target);
                //conn.Send(InsightChatMessage.MsgId, message);
                break;

            //Send message to all players on same Zone
            case (short)ChatChannelType.Public:
                Debug.Log("Type: " + message.ChannelType + " Origin: " + message.Origin + " Message: " + message.Message);
                //How to find what zone an player is in?
                break;

            //Send to all players on all zones (admin only later)
            case (short)ChatChannelType.Global:
                Debug.Log("Type: " + message.ChannelType + " Origin: " + message.Origin + " Message: " + message.Message);
                //How to find all players logged into all zones.
                break;

            case (short)ChatChannelType.Party:
                Debug.Log("Type: " + message.ChannelType + " Origin: " + message.Origin + " Message: " + message.Message);
                break;

            case (short)ChatChannelType.Guild:
                Debug.Log("Type: " + message.ChannelType + " Origin: " + message.Origin + " Message: " + message.Message);
                break;

            default:
                Debug.LogError("Received unexpected ChatChannelType");
                break;
        }
    }
}

public class InsightChatMessage : MessageBase
{
    public static short MsgId = 9999;
    public short ChannelType;
    public string Message;
    public string Origin; 
    public string Target; //Used for private messages
}

public enum ChatChannelType : short
{
    Private,
    Public,
    Global,
    Party,
    Guild
};
