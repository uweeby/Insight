using Insight;

public class ClientLoginModule : InsightModule
{
    InsightCommon insight;

    public override void Initialize(InsightCommon insight)
    {
        this.insight = insight;

        RegisterHandlers();
    }

    public override void RegisterHandlers()
    {
        
    }
}
