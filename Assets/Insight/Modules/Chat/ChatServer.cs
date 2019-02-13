using Insight;
using UnityEngine;

public class ChatServer : InsightModule
{
    public InsightServer server;
    public ServerAuthentication authModule;

    public void Awake()
    {
        AddOptionalDependency<ServerAuthentication>();
    }
    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;

        if(manager.GetModule<ServerAuthentication>() != null)
        {
            authModule = manager.GetModule<ServerAuthentication>();
        }

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.Chat, HandleChatMsg);
    }

    private void HandleChatMsg(InsightNetworkMessage netMsg)
    {
        if(server.logNetworkMessages) { Debug.Log("[ChatServer] - Received Chat Message."); }

        ChatMsg message = netMsg.ReadMessage<ChatMsg>();

        if (authModule != null)
        {
            //Find the user
            UserContainer user = authModule.GetUserByConnection(netMsg.connectionId);

            //Inject the username into the message
            message.Origin = user.username;

            server.SendToAll((short)MsgId.Chat, message);
        }

        //No Authentication Module. Simple Echo
        else
        {
            //Broadcast back to all other clients
            server.SendToAll((short)MsgId.Chat, message);
        }
    }
}
