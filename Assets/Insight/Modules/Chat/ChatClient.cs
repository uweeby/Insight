using Insight;
using Mirror;
using UnityEngine;

public class ChatClient : InsightModule
{
    static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerAuthentication));

    NetworkClient client;

    //Used in Example Scene:
    [HideInInspector] public string chatLog;

    public override void Initialize(NetworkClient client, ModuleManager manager)
    {
        this.client = client;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.Connection.RegisterHandler<ChatMsg>(HandleChatMsg);
    }

    public void HandleChatMsg(ChatMsg netMsg)
    {
        if (logger.LogEnabled()) logger.Log("[InsightClient] - HandleChatMsg()");

        chatLog += netMsg.Origin + ": "  + netMsg.Data + "\n";
    }

    //Has server control the username (MasterServer Example)
    public void SendChatMsg(string data)
    {
        client.Send((short)MsgId.Chat, new ChatMsg() { Data = data });
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
