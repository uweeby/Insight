using Insight;

public class ChatInsightServer : InsightServer
{
    public bool showDebugMessages;

    // Use this for initialization
    public override void Start ()
    {
        base.Start();

        RegisterHandlers();
    }

    // Update is called once per frame
    public override void Update ()
    {
        HandleNewMessages();
    }

    private void RegisterHandlers()
    {
        RegisterHandler(ChatMessage.MsgId, HandleChatMessage);
    }

    private void HandleChatMessage(InsightNetworkMessage netMsg)
    {
        print("HandleChatMessage");

        ChatMessage message = netMsg.ReadMessage<ChatMessage>();

        print("Data: " + message.Data);
    }
}