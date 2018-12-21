using Insight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientChatModule : InsightModule
{
    InsightCommon insight;

    public override void Initialize(InsightCommon insight)
    {
        this.insight = insight;
    }

    public override void RegisterHandlers()
    {
        
    }
}
