using System.Collections.Generic;
using Insight;

public class ClientGameRequest : InsightModule
{
    InsightClient client;

    public List<GameContainer> registeredGames = new List<GameContainer>();

    public override void Initialize(InsightClient insight, ModuleManager manager)
    {
        client = insight;
        RegisterHandlers();
    }

    void RegisterHandlers()
    {

    }

    public void SendGetGamesListMsg()
    {
        client.Send((short)MsgId.RequestGame, new PropertiesMsg() {  }); //you should also be able to specify options
    }
}
