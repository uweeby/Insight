using Insight;
using UnityEngine;

public class ServerChatModule : InsightModule
{
    InsightCommon insight;
    ModuleManager manager;

    public override void Initialize(InsightCommon insight, ModuleManager manager)
    {
        this.insight = insight;
        this.manager = manager;

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