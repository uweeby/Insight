using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Insight
{
    public class ServerAuthentication : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerAuthentication));

        NetworkServer server;

        public List<UserContainer> registeredUsers = new List<UserContainer>();

        public override void Initialize(NetworkServer server, ModuleManager manager)
        {
            this.server = server;

            RegisterHandlers();

            server.Disconnected.AddListener(HandleDisconnect);
        }

        void RegisterHandlers()
        {
            server.LocalConnection.RegisterHandler<LoginMsg>(HandleLoginMsg);
        }

        //This is just an example. No actual authentication happens.
        //You would need to replace with your own logic. Perhaps with a DB connection.
        void HandleLoginMsg(INetworkConnection conn, LoginMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[Authentication] - Login Received: " + netMsg.AccountName + " / " + netMsg.AccountPassword);

            //Login Sucessful
            if (true) //Put your DB logic here
            {
                string UniqueId = Guid.NewGuid().ToString();

                registeredUsers.Add(new UserContainer()
                {
                    username = netMsg.AccountName,
                    uniqueId = UniqueId,
                    connection = conn
                });

                conn.SendAsync(new LoginResponseMsg() //This was a reply
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

        void HandleDisconnect(INetworkConnection conn)
        {
            foreach (UserContainer user in registeredUsers)
            {
                if (user.connection == conn)
                {
                    registeredUsers.Remove(user);
                    return;
                }
            }
        }

        public UserContainer GetUserByConnection(INetworkConnection conn)
        {
            foreach (UserContainer user in registeredUsers)
            {
                if (user.connection == conn)
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
        public INetworkConnection connection;
    }
}
