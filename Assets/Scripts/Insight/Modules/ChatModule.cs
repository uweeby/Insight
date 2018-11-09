using Insight;
using Mirror;

public class ChatModule : InsightModule
{
    InsightServer serverInstance;

    public override void Initialize(InsightServer server)
    {

    }

    public override void RegisterHandlers()
    {
        serverInstance.RegisterHandler(ChatMessage.MsgId, HandleChatMessage);
    }

    private void HandleChatMessage(InsightNetworkMessage netMsg)
    {

    }
}

public class ChatMessage : MessageBase
{
    public static short MsgId = 9999;
    public string Message;
}
