using Insight;
using UnityEngine;

public class ArgsModule : InsightModule
{
    InsightCommon insight;
    ModuleManager manager;

    private InsightArgs insightArguments = new InsightArgs();

    public override void Initialize(InsightCommon insight, ModuleManager manager)
    {
        this.insight = insight;
        this.manager = manager;

        RegisterHandlers();

        SetupWithArgs();
    }

    public override void RegisterHandlers()
    {
        
    }

    private void SetupWithArgs()
    {
        if (insightArguments.IsProvided("-AssignedPort"))
        {
            insight.networkPort = insightArguments.ExtractValueInt("-AssignedPort");
            Debug.Log("[ArgsModule] - Set Port: " + insight.networkPort);
        }
    }
}