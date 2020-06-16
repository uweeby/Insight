using Insight;
using Mirror;
using UnityEngine;

public class ChatClient : InsightModule
{
    static readonly ILogger logger = LogFactory.GetLogger(typeof(ChatClient));

    InsightClient client;

    //Used in Example Scene:
    [HideInInspector] public string chatLog;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler<ChatMsg>(HandleChatMsg);
    }

    public void HandleChatMsg(InsightNetworkMessage netMsg)
    {
        logger.Log("[InsightClient] - HandleChatMsg()");

        ChatMsg message = netMsg.ReadMessage<ChatMsg>();

        chatLog += message.Origin + ": "  + message.Data + "\n";
    }

    //Has server control the username (MasterServer Example)
    public void SendChatMsg(string data)
    {
        client.Send(new ChatMsg() { Data = data });
    }

    //Allows the user to set their own name (Chat Example)
    public void SendChatMsg(string Origin, string Data)
    {
        client.Send(new ChatMsg()
        {
            Origin = Origin,
            Data = Data
        });
    }
}
