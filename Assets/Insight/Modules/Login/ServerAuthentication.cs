using Insight;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerAuthentication : InsightModule
{
    InsightServer server;

    public List<UserContainer> registeredUsers = new List<UserContainer>();

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.Login, HandleLoginMsg);
    }

    //You would probably want to use a database to check for accounts that are already registered
    //Since this is a simple example we will just assume any new login is valid.
    private void HandleLoginMsg(InsightNetworkMessage netMsg)
    {
        LoginMsg message = netMsg.ReadMessage<LoginMsg>();

        if (server.logNetworkMessages) { Debug.Log("[InsightServer] - Login Received: " + message.AccountName + " / " + message.AccountPassword); }

        //Add your own code to verify the user/pass are correct

        //For demo purposes just accept anything. THIS IS BAD PRACTICE REPLACE THIS CODE IN YOUR GAME.
        registeredUsers.Add(new UserContainer() { username = message.AccountName, uniqueId = Guid.NewGuid().ToString() });
        netMsg.Reply((short)MsgId.Status, new StatusMsg() { Text = "Login Sucessful!" });
    }
}

public struct UserContainer
{
    public string uniqueId;
    public string username;
}
