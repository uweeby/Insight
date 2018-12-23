using Insight;

public class SQLiteModule : InsightModule
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
        
    }
}
