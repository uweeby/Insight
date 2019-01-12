using Insight;
using UnityEngine;

public class ClientChatModule : InsightModule
{
    InsightClient client;

    //Used in Example Scene:
    public ChatGUI chatGuiComp;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler(ChatMessage.MsgId, HandleChatMessage);
    }

    public void HandleChatMessage(InsightNetworkMessage netMsg)
    {
        if (client.logNetworkMessages) { Debug.Log("[InsightClient] - HandleChatMessage()"); }

        ChatMessage message = netMsg.ReadMessage<ChatMessage>();

        chatGuiComp.textField.text += message.Origin + ": "  + message.Data + "\n";
    }
}
