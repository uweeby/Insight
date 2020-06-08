using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public class ServerAuthentication : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerAuthentication));

        InsightServer server;
        public NetworkAuthenticator authenticator;

        public List<UserContainer> registeredUsers = new List<UserContainer>();

        public override void Initialize(InsightServer server, ModuleManager manager)
        {
            this.server = server;

            RegisterHandlers();

            server.transport.OnServerDisconnected.AddListener(HandleDisconnect);
        }

        void RegisterHandlers()
        {
            server.RegisterHandler<LoginMsg>(HandleLoginMsg);
        }

        //This is just an example. No actual authentication happens.
        //You would need to replace with your own logic. Perhaps with a DB connection.
        void HandleLoginMsg(InsightNetworkMessage netMsg)
        {
            LoginMsg message = netMsg.ReadMessage<LoginMsg>();

            if (server.logNetworkMessages) { logger.Log("[Authentication] - Login Received: " + message.AccountName + " / " + message.AccountPassword); }

            //Login Sucessful
            if (true) //Put your DB logic here
            {
                string UniqueId = Guid.NewGuid().ToString();

                registeredUsers.Add(new UserContainer()
                {
                    username = message.AccountName,
                    uniqueId = UniqueId,
                    connectionId = netMsg.connectionId
                });

                netMsg.Reply(new LoginResponseMsg()
                {
                    Authenticated = true,
                    UniqueID = UniqueId
                });
            }

            //Login Failed. Unreachable code currently as there is no real auth happening.
            //else
            //{
            //    netMsg.Reply((short)MsgId.LoginResponse, new LoginResponseMsg()
            //    {
            //        Authenticated = false
            //    });
            //}
        }

        void HandleDisconnect(int connectionId)
        {
            foreach (UserContainer user in registeredUsers)
            {
                if (user.connectionId == connectionId)
                {
                    registeredUsers.Remove(user);
                    return;
                }
            }
        }

        public UserContainer GetUserByConnection(int connectionId)
        {
            foreach (UserContainer user in registeredUsers)
            {
                if (user.connectionId == connectionId)
                {
                    return user;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class UserContainer
    {
        public string uniqueId;
        public string username;
        public int connectionId;
    }
}
