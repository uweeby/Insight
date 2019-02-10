using Insight;
using UnityEngine;

public class ChatClient : InsightModule
{
    InsightClient client;

    //Used in Example Scene:
    [HideInInspector]
    public string chatLog;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler((short)MsgId.Chat, HandleChatMsg);
    }

    public void HandleChatMsg(InsightNetworkMessage netMsg)
    {
        if (client.logNetworkMessages) { Debug.Log("[InsightClient] - HandleChatMsg()"); }

        ChatMsg message = netMsg.ReadMessage<ChatMsg>();

        chatLog += message.Origin + ": "  + message.Data + "\n";
    }
}
