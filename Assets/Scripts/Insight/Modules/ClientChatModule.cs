using Insight;

public class ClientChatModule : InsightModule
{
    InsightCommon insight;

    public override void Initialize(InsightCommon insight)
    {
        this.insight = insight;
    }

    public override void RegisterHandlers()
    {
        insight.RegisterHandler(InsightChatMessage.MsgId, HandleChatMessage);
    }

    public void HandleChatMessage(InsightNetworkMessage netMsg)
    {

    }

    public void SendChatMessage(string Data)
    {
        
    }
}
