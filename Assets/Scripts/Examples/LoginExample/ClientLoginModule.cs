using Insight;
using UnityEngine;

public class ClientLoginModule : InsightModule
{
    InsightCommon insight;
    ModuleManager manager;

    public LoginGUI loginGuiComp;

    public override void Initialize(InsightCommon insight, ModuleManager manager)
    {
        this.insight = insight;
        this.manager = manager;

        RegisterHandlers();
    }

    public override void RegisterHandlers()
    {
        insight.RegisterHandler(StatusMsg.MsgId, HandleStatusMsg);
    }

    private void HandleStatusMsg(InsightNetworkMessage netMsg)
    {
        if (insight.logNetworkMessages) { Debug.Log("[InsightClient] - HandleStatusMsg()"); }

        StatusMsg message = netMsg.ReadMessage<StatusMsg>();

        //Added for Demo Scene
        loginGuiComp.statusText.text = message.Text;
    }
}