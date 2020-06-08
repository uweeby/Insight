using UnityEngine;

namespace Insight
{
    public class ChatServer : InsightModule
    {
        InsightServer server;
        ServerAuthentication authModule;

        public void Awake()
        {
            AddOptionalDependency<ServerAuthentication>();
        }
        public override void Initialize(InsightServer server, ModuleManager manager)
        {
            this.server = server;

            if (manager.GetModule<ServerAuthentication>() != null)
            {
                authModule = manager.GetModule<ServerAuthentication>();
            }

            RegisterHandlers();
        }

        void RegisterHandlers()
        {
            server.RegisterHandler<ChatMsg>(HandleChatMsg);
        }

        void HandleChatMsg(InsightNetworkMessage netMsg)
        {
            if (server.logNetworkMessages) { logger.Log("[ChatServer] - Received Chat Message."); }

            ChatMsg message = netMsg.ReadMessage<ChatMsg>();

            if (authModule != null)
            {
                //Inject the username into the message
                message.Origin = authModule.GetUserByConnection(netMsg.connectionId).username;

                foreach(UserContainer user in authModule.registeredUsers)
                {
                    server.SendToClient(user.connectionId, message);
                }
            }

            //No Authentication Module. Simple Echo
            else
            {
                //Broadcast back to all other clients
                server.SendToAll(message);
            }
        }
    }
}
