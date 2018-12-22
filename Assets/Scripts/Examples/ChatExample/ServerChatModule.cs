using Insight;
using UnityEngine;

public class ServerChatModule : InsightModule
{
    InsightCommon insight;

    public override void Initialize(InsightCommon insight)
    {
        this.insight = insight;

        RegisterHandlers();
    }

    public override void RegisterHandlers()
    {
        insight.RegisterHandler(ChatMessage.MsgId, HandleChatMessage);
    }

    private void HandleChatMessage(InsightNetworkMessage netMsg)
    {
        if(insight.logNetworkMessages) { Debug.Log("[InsightServer] - HandleChatMessage()"); }

        ChatMessage message = netMsg.ReadMessage<ChatMessage>();

        insight.SendMsgToAll(ChatMessage.MsgId, message); //Broadcast back to all other clients
    }
}