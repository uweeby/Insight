using Insight;
using UnityEngine;

public class ChatServer : InsightModule
{
    InsightServer server;

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.Chat, HandleChatMsg);
    }

    private void HandleChatMsg(InsightNetworkMessage netMsg)
    {
        if(server.logNetworkMessages) { Debug.Log("[InsightServer] - HandleChatMsg()"); }

        ChatMsg message = netMsg.ReadMessage<ChatMsg>();

        server.SendToAll((short)MsgId.Chat, message); //Broadcast back to all other clients
    }
}
