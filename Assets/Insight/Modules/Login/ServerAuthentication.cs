using Insight;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerAuthentication : InsightModule
{
    InsightServer server;
    public TelepathyTransport telepathyTransport;

    public List<UserContainer> registeredUsers = new List<UserContainer>();

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;

        RegisterHandlers();

        server.transport.OnServerDisconnected.AddListener(HandleDisconnect);
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.Login, HandleLoginMsg);
    }

    //This is just an example. No actual authentication happens.
    //You would need to replace with your own logic. Perhaps with a DB connection.
    private void HandleLoginMsg(InsightNetworkMessage netMsg)
    {
        LoginMsg message = netMsg.ReadMessage<LoginMsg>();
        
        if (server.logNetworkMessages) { Debug.Log("[InsightServer] - Login Received: " + message.AccountName + " / " + message.AccountPassword); }

        registeredUsers.Add(new UserContainer()
        {
            username = message.AccountName,
            uniqueId = Guid.NewGuid().ToString(),
            connectionId = netMsg.connectionId
        });

        netMsg.Reply((short)MsgId.Status, new StatusMsg() { Text = "Login Sucessful!" });
    }

    private void HandleDisconnect(int connectionId)
    {
        foreach(UserContainer user in registeredUsers)
        {
            if(user.connectionId == connectionId)
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
            if(user.connectionId == connectionId)
            {
                return user;
            }
        }
        return null;
    }
}

public class UserContainer
{
    public string uniqueId;
    public string username;
    public int connectionId;
}
