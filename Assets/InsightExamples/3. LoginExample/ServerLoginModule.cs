using Insight;
using UnityEngine;

public class ServerLoginModule : InsightModule
{
    InsightServer server;

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler(LoginMsg.MsgId, HandleLoginMsg);
    }

    private void HandleLoginMsg(InsightNetworkMessage netMsg)
    {
        if (server.logNetworkMessages) { Debug.Log("[InsightServer] - HandleLoginMsg()"); }

        LoginMsg message = netMsg.ReadMessage<LoginMsg>();

        if (server.logNetworkMessages) { Debug.Log("[InsightServer] - Login Received: " + message.AccountName + " / " + message.AccountPassword); }

        //Add code to verify the user/pass are correct

        //We expect the SHA256 for 'password'. Its not salted with anything currently
        if (message.AccountName.Equals("root") && message.AccountPassword.Equals("5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8"))
        {
            netMsg.Reply(StatusMsg.MsgId, new StatusMsg() { Text = "Login Sucessful!" });
        }
        else
        {
            netMsg.Reply(StatusMsg.MsgId, new StatusMsg() { Text = "Login Failed!" });
        }
    }
}