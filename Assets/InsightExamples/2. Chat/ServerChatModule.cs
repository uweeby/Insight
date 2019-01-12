using Insight;
using UnityEngine;

public class ServerChatModule : InsightModule
{
    InsightServer server;

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler(ChatMessage.MsgId, HandleChatMessage);
    }

    private void HandleChatMessage(InsightNetworkMessage netMsg)
    {
        if(server.logNetworkMessages) { Debug.Log("[InsightServer] - HandleChatMessage()"); }

        ChatMessage message = netMsg.ReadMessage<ChatMessage>();

        server.SendToAll(ChatMessage.MsgId, message); //Broadcast back to all other clients
    }
}