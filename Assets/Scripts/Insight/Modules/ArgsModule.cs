using Insight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArgsModule : InsightModule
{
    InsightCommon insight;

    private InsightArgs insightArguments = new InsightArgs();

    public override void Initialize(InsightCommon insight)
    {
        this.insight = insight;

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