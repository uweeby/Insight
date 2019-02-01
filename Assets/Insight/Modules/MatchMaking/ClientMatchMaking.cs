using Insight;

public class ClientMatchMaking : InsightModule
{
    InsightClient client;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        
    }

    public void SendMatchRequest()
    {
        client.Send((short)MsgId.RequestMatch, new RequestMatch() { GameType = 0, GameSlots = 16 });
    }
}
