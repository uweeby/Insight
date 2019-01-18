using Insight;
using UnityEngine;

public class ServerMatchMakerModule : InsightModule
{
    InsightServer server;

    //List of all players looking for a Match
    //List of all games/spawners available to spawn a new Match

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler(FindMatch.MsgId, HandleFindMatch);
    }

    private void HandleFindMatch(InsightNetworkMessage netMsg)
    {
        FindMatch message = netMsg.ReadMessage<FindMatch>();


    }
}
