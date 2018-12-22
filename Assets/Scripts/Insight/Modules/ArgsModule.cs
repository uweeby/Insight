using Insight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArgsModule : InsightModule
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