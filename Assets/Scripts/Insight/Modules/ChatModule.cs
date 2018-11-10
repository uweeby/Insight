using Insight;
using Mirror;
using UnityEngine;

public class ChatModule : InsightModule
{
    InsightServer insightServer;

    public override void Initialize(InsightServer server)
    {
        insightServer = server;
    }

    public override void RegisterHandlers()
    {
        insightServer.RegisterHandler(ChatMessage.MsgId, HandleChatMessage);
    }

    private void HandleChatMessage(InsightNetworkMessage netMsg)
    {
        ChatMessage message = netMsg.ReadMessage<ChatMessage>();

        //Find the Chat Code
        if(message.Message.Substring(0, 2).Equals("/t"))
        {
            string player = message.Message.Substring(3, message.Message.IndexOf(" ", 4));
            Debug.Log("Received Tell: " + player);
        }
        if (message.Message.Substring(0, 2).Equals("/s"))
        {
            string data = message.Message.Substring(3, message.Message.Length - 3);
            Debug.Log("Received Local: " + data);
        }
        if (message.Message.Substring(0, 2).Equals("/y"))
        {
            string data = message.Message.Substring(3, message.Message.Length - 3);
            Debug.Log("Received Global: " + data);
        }
        if (message.Message.Substring(0, 2).Equals("/g"))
        {
            string data = message.Message.Substring(3, message.Message.Length - 3);
            Debug.Log("Received Guild: " + data);
        }
        if (message.Message.Substring(0, 2).Equals("/p"))
        {
            string data = message.Message.Substring(3, message.Message.Length - 3);
            Debug.Log("Received party: " + data);
        }
    }
}

public class ChatMessage : MessageBase
{
    public static short MsgId = 9999;
    public string Message;
}
