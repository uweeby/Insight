using UnityEngine;
using Mirror;

namespace Insight
{
    public class ChatServer : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ChatServer));

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

            server.Authenticated.AddListener(RegisterHandlers);
        }

        void RegisterHandlers(INetworkConnection conn)
        {
            conn.RegisterHandler<ChatMsg>(HandleChatMsg);
        }

        void HandleChatMsg(INetworkConnection conn, ChatMsg netMsg)
        {
            logger.Log("[ChatServer] - Received Chat Message.");

            if (authModule != null)
            {
                //Inject the username into the message
                netMsg.Origin = authModule.GetUserByConnection(conn).username;

                foreach(UserContainer user in authModule.registeredUsers)
                {
                    user.connection.SendAsync(netMsg);
                }
            }

            //No Authentication Module. Simple Echo
            else
            {
                //Broadcast back to all other clients
                server.SendToAll(netMsg);
            }
        }
    }
}
