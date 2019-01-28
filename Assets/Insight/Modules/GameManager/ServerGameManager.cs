using Insight;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameManager : InsightModule
{
    InsightServer server;
    MasterSpawner masterSpawner;

    public List<GameContainer> registeredGames = new List<GameContainer>();

    public void Awake()
    {
        AddDependency<MasterSpawner>();
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        masterSpawner = manager.GetModule<MasterSpawner>();
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.RegisterGame, HandleRegisterGameMsg);
        server.RegisterHandler((short)MsgId.RequestMatch, HandleRequestMatchMsg);
    }

    private void HandleRegisterGameMsg(InsightNetworkMessage netMsg)
    {
        RegisterGameMsg message = netMsg.ReadMessage<RegisterGameMsg>();

        if (server.logNetworkMessages) { Debug.Log("Received GameRegistration request"); }

        registeredGames.Add(new GameContainer() { connectionId = netMsg.connectionId, uniqueId = message.UniqueID });
    }

    private void HandleRequestMatchMsg(InsightNetworkMessage netMsg)
    {
        //GamesList message = netMsg.ReadMessage<GamesList>();

        List<GameContainer> gamesMeetingCriteria = new List<GameContainer>();

        //Check the local collection of Registered Games.
        foreach(GameContainer game in registeredGames)
        {
            //if(game.Properties.ContainsKey("GameType").Equals("SomeAwesomeGame"))
            //{
            //    gamesMeetingCriteria.Add(game);
            //}
        }

        //If a game meeting the options criteria is not found. Request a spawn.
        if(gamesMeetingCriteria.Count == 0)
        {
            masterSpawner.RequestGameSpawn(); //Add the options here

            //Should this also be a callback?

            //Somehow the list needs to get populated
        }

        //Reply to fulfil the callback request.
        //netMsg.Reply((short)MsgId.FindGame,  { }); //Send gamesMeetingCriteria back to fulfil callback.
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
    public Dictionary<string, string> Properties;
}
