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

    public override void RegisterHandlers()
    {
        server.RegisterHandler(LoginMsg.MsgId, HandleLoginMsg);
    }

    private void HandleLoginMsg(InsightNetworkMessage netMsg)
    {
        if (server.logNetworkMessages) { Debug.Log("[InsightServer] - HandleLoginMsg()"); }

        LoginMsg message = netMsg.ReadMessage<LoginMsg>();

        if (server.logNetworkMessages) { Debug.Log("[InsightServer] - Login Received: " + message.AccountName + " / " + message.AccountPassword); }

        //Add code to verify the user/pass are correct
        //Yes the passwords are in plain text for this demo!
        if (message.AccountName.Equals("root") && message.AccountPassword.Equals("password"))
        {
            netMsg.Reply(StatusMsg.MsgId, new StatusMsg() { Text = "Login Sucessful!" });
        }
        else
        {
            netMsg.Reply(StatusMsg.MsgId, new StatusMsg() { Text = "Login Failed!" });
        }
    }
}