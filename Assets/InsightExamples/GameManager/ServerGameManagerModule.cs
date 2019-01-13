using Insight;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameManagerModule : InsightModule
{
    InsightServer server;

    public List<GameContainer> registeredGames = new List<GameContainer>();

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler(RegisterGame.MsgId, HandleRegisterGame);
    }

    private void HandleRegisterGame(InsightNetworkMessage netMsg)
    {

    }
}

public struct GameContainer
{
    public string uniqueId;
    public int connectionId;
}