using Insight;
using UnityEngine;

public class ClientLoginModule : InsightModule
{
    InsightClient client;

    public LoginGUI loginGuiComp;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        RegisterHandlers();
    }

    public override void RegisterHandlers()
    {
        client.RegisterHandler(StatusMsg.MsgId, HandleStatusMsg);
    }

    private void HandleStatusMsg(InsightNetworkMessage netMsg)
    {
        if (client.logNetworkMessages) { Debug.Log("[InsightClient] - HandleStatusMsg()"); }

        StatusMsg message = netMsg.ReadMessage<StatusMsg>();

        //Added for Demo Scene
        loginGuiComp.statusText.text = message.Text;
    }
}