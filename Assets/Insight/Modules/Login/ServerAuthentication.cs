using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public class ServerAuthentication : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerAuthentication));

        public bool EnforceAuthentication; //Only enabled on the 'Login' example for ease of use of other examples.

        InsightServer server;

        public List<UserContainer> registeredUsers = new List<UserContainer>();

        public override void Initialize(InsightServer server, ModuleManager manager)
        {
            this.server = server;

            RegisterHandlers();

            server.transport.OnServerDisconnected=HandleDisconnect;
        }

        void RegisterHandlers()
        {
            server.RegisterHandler<LoginMsg>(HandleLoginMsg);
        }

        //This is a very simple very bad example.
        //You would need to replace with your own logic. Perhaps with a DB connection.
        void HandleLoginMsg(InsightNetworkMessage netMsg)
        {
            LoginMsg message = netMsg.ReadMessage<LoginMsg>();

            logger.Log("[ServerAuthentication] - Login Received: " + message.AccountName + " / " + message.AccountPassword);

            if(EnforceAuthentication)
            {
                //Check the username and password. Again this is bad code for example only. REPLACE ME
                if (message.AccountName.Equals("root") && message.AccountPassword.Equals("password"))
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
                        Status = CallbackStatus.Success,
                        UniqueID = UniqueId
                    });
                }

                //Login Failed
                else
                {
                    netMsg.Reply(new LoginResponseMsg()
                    {
                        Status = CallbackStatus.Error
                    });
                }
            }
            else
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
                    Status = CallbackStatus.Success
                });
            }
            
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
