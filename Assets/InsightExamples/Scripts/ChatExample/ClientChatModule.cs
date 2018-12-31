using Insight;
using UnityEngine;

public class ClientChatModule : InsightModule
{
    InsightCommon insight;
    ModuleManager manager;

    //Used in Example Scene:
    public ChatGUI chatGuiComp;

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

    public void HandleChatMessage(InsightNetworkMessage netMsg)
    {
        if (insight.logNetworkMessages) { Debug.Log("[InsightClient] - HandleChatMessage()"); }

        ChatMessage message = netMsg.ReadMessage<ChatMessage>();

        chatGuiComp.textField.text += message.Origin + ": "  + message.Data + "\n";
    }
}
