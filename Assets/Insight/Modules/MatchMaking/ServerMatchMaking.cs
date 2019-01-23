using System.Collections.Generic;
using Insight;
using UnityEngine;

public class ServerMatchMaking : InsightModule
{
    InsightServer server;
    //ModuleManager manager;
    //ServerGameManagerModule gameManager;

    List<UserSeekingMatch> usersSeekingMatchList = new List<UserSeekingMatch>();

    public void Awake()
    {
        AddDependency<ServerGameManager>();
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        //this.manager = manager;
        //gameManager = this.manager.GetModule<ServerGameManagerModule>();
        RegisterHandlers();

        InvokeRepeating("UpdateMatches", 10f, 10f);
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.RequestMatch, HandleRequestMatchMsg);
    }

    private void HandleRequestMatchMsg(InsightNetworkMessage netMsg)
    {
        //PropertiesMsg message = netMsg.ReadMessage<PropertiesMsg>();

        //Add player to list of players looking for a match
        usersSeekingMatchList.Add(new UserSeekingMatch() { playerName = "Player01", gameType = "FFA" });

    }

    private void UpdateMatches()
    {
        //Check the list of players and current games at specified interval
    }

    public struct UserSeekingMatch
    {
        //Placeholder
        public string playerName;
        public string gameType;
    }
}
