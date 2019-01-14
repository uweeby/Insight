using Insight;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameManagerModule : InsightModule
{
    InsightServer server;
    ModuleManager manager;
    MasterSpawner masterSpawner;

    public List<GameContainer> registeredGames = new List<GameContainer>();

    public void Awake()
    {
        AddDependency<MasterSpawner>();
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        this.manager = manager;
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler(RegisterGame.MsgId, HandleRegisterGame);
        server.RegisterHandler(GamesList.MsgId, HandleGamesList);
    }

    private void HandleRegisterGame(InsightNetworkMessage netMsg)
    {
        RegisterGame message = netMsg.ReadMessage<RegisterGame>();

        registeredGames.Add(new GameContainer() { connectionId = netMsg.connectionId, uniqueId = message.UniqueID });
    }

    private void HandleGamesList(InsightNetworkMessage netMsg)
    {
        GamesList message = netMsg.ReadMessage<GamesList>();

        List<GameContainer> gamesMeetingCriteria = new List<GameContainer>();

        //Check the local collection of Registered Games.
        foreach(GameContainer game in registeredGames)
        {
            if(game.GameType.Equals("SomeAwesomeGame"))
            {
                gamesMeetingCriteria.Add(game);
            }
        }

        //If a game meeting the options criteria is not found. Request a spawn.
        if(gamesMeetingCriteria.Count == 0)
        {
            masterSpawner.RequestGameSpawn(); //Add the options here

            //Should this also be a callback?

            //Somehow the list needs to get populated
        }

        //Reply to fulfil the callback request.
        netMsg.Reply(GamesList.MsgId, new GamesList() { }); //Send gamesMeetingCriteria back to fulfil callback.
    }
}

public partial class MasterSpawner
{
    public void RequestGameSpawn() //Take in the options here
    {
    }
}

public struct GameContainer
{
    public string uniqueId;
    public int connectionId;
    public string GameType;
}