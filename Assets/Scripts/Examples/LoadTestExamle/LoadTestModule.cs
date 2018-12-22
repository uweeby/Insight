using Insight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTestModule : InsightModule
{
    InsightCommon insight;

    //Used in Example Scene:
    
    public override void Initialize(InsightCommon insight)
    {
        this.insight = insight;

        RegisterHandlers();
    }

    public override void RegisterHandlers()
    {

    }
}
