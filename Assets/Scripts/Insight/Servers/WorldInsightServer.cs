using Insight;

public class WorldInsightServer : InsightServer
{
    // Use this for initialization
    public override void Start ()
    {        
        RegisterHandlers();
    }

    // Update is called once per frame
    public override void Update ()
    {
        HandleNewMessages();
    }

    private void RegisterHandlers()
    {
        
    }
}
