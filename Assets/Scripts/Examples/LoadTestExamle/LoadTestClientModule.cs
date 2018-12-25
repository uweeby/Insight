using Insight;
using UnityEngine;

public class LoadTestClientModule : InsightModule
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
        insight.RegisterHandler(ClientLoadTestMsg.MsgId, HandleClientLoadTestMsg);
        insight.RegisterHandler(ServerLoadTestMsg.MsgId, HandleServerLoadTestMsg);

    }

    private void HandleClientLoadTestMsg(InsightNetworkMessage netMsg)
    {
        //Msg that was echoed back from a client via the server.
    }

    private void HandleServerLoadTestMsg(InsightNetworkMessage netMsg)
    {
        //Msg sent direct from server.
    }
}
