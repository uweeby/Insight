using Insight;

public class SQLiteModule : InsightModule
{
    InsightClient insight;
    ModuleManager manager;

    public override void Initialize(InsightClient insight, ModuleManager manager)
    {
        this.insight = insight;
        this.manager = manager;

        RegisterHandlers();
    }

    public override void RegisterHandlers()
    {
        
    }
}
