using UnityEngine;
using Mirror;

namespace Insight
{
    public class ChatServer : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerAuthentication));

        NetworkServer server;
        ServerAuthentication authModule;

        public void Awake()
        {
            AddOptionalDependency<ServerAuthentication>();
        }
        public override void Initialize(NetworkServer server, ModuleManager manager)
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
            server.LocalConnection.RegisterHandler<ChatMsg>(HandleChatMsg);
        }

        void HandleChatMsg(INetworkConnection conn, ChatMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[ChatServer] - Received Chat Message.");

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
