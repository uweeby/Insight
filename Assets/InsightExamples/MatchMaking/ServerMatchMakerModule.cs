using System.Collections.Generic;
using Insight;
using UnityEngine;

public class ServerMatchMakerModule : InsightModule
{
    InsightServer server;
    ModuleManager manager;
    ServerGameManagerModule gameManager;

    //List of all players looking for a Match
    //List of all games. If a new game needs to be spawned it should ask the GameManager to do it.
    List<UserSeekingMatch> usersSeekingMatchList = new List<UserSeekingMatch>();

    public void Awake()
    {
        AddDependency<ServerGameManagerModule>();
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        this.manager = manager;
        gameManager = this.manager.GetModule<ServerGameManagerModule>();
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler(FindMatch.MsgId, HandleFindMatch);
    }

    private void HandleFindMatch(InsightNetworkMessage netMsg)
    {
        FindMatch message = netMsg.ReadMessage<FindMatch>();

        //Add player to list of players looking for a match
        usersSeekingMatchList.Add(new UserSeekingMatch() { playerName = "Player01", gameType = "FFA" });

    }

    public struct UserSeekingMatch
    {
        //Placeholder
        public string playerName;
        public string gameType;
    }
}
