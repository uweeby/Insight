using Insight;
using UnityEngine;

public class ServerLoginModule : InsightModule
{
    InsightCommon insight;
    ModuleManager manager;

    public override void Initialize(InsightCommon insight, ModuleManager manager)
    {
        this.insight = insight;
        this.manager = manager;

        RegisterHandlers();
    }

    public override void RegisterHandlers()
    {
        insight.RegisterHandler(LoginMsg.MsgId, HandleLoginMsg);
    }

    private void HandleLoginMsg(InsightNetworkMessage netMsg)
    {
        if (insight.logNetworkMessages) { Debug.Log("[InsightServer] - HandleLoginMsg()"); }

        LoginMsg message = netMsg.ReadMessage<LoginMsg>();

        if (insight.logNetworkMessages) { Debug.Log("[InsightServer] - Login Received: " + message.AccountName + " / " + message.AccountPassword); }

        //Add code to verify the user/pass are correct
        //Yes the passwords are in plain text for this demo!
        if (message.AccountName.Equals("root") && message.AccountPassword.Equals("password"))
        {
            netMsg.conn.Send(StatusMsg.MsgId, new StatusMsg() { Text = "Login Sucessful!" });
        }
        else
        {
            netMsg.conn.Send(StatusMsg.MsgId, new StatusMsg() { Text = "Login Failed!" });
        }
    }
}