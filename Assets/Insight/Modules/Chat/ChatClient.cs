using Insight;
using UnityEngine;

//TODO: Remove the example specific code from module

public class ChatClient : InsightModule
{
    public InsightClient client;

    //Used in Example Scene:
    [HideInInspector] public string chatLog;

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

    //Has server control the username (MasterServer Example)
    public void SendChatMsg(string Origin)
    {
        client.Send((short)MsgId.Chat, new ChatMsg() { Origin = Origin});
    }

    //Allows the user to set their own name (Chat Example)
    public void SendChatMsg(string Origin, string Data)
    {
        client.Send((short)MsgId.Chat, new ChatMsg()
        {
            Origin = Origin,
            Data = Data
        });
    }
}
