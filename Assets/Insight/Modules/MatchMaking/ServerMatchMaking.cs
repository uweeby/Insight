using Insight;
using System;
using System.Collections.Generic;

public class ServerMatchMaking : InsightModule
{
    InsightServer server;
    ModuleManager manager;
    ServerAuthentication authModule;
    ServerGameManager gameManager;
    MasterSpawner masterSpawner;

    public int MinimumPlayersForGame = 1;
    public float MatchMakingPollRate = 10f;

    public List<UserContainer> usersInQueue = new List<UserContainer>();
    public List<MatchContainer> matchList = new List<MatchContainer>();

    public void Awake()
    {
        AddDependency<MasterSpawner>();
        AddDependency<ServerAuthentication>(); //Used to track logged in players
        AddDependency<ServerGameManager>(); //Used to track available games
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        this.manager = manager;
        authModule = this.manager.GetModule<ServerAuthentication>();
        gameManager = this.manager.GetModule<ServerGameManager>();
        masterSpawner = this.manager.GetModule<MasterSpawner>();

        RegisterHandlers();

        InvokeRepeating("CheckQueue", MatchMakingPollRate, MatchMakingPollRate);
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.StartMatchMaking, HandleStartMatchSearchMsg);
        server.RegisterHandler((short)MsgId.StopMatchMaking, HandleStopMatchSearchMsg);
    }

    private void HandleStartMatchSearchMsg(InsightNetworkMessage netMsg)
    {
        if (server.logNetworkMessages) { UnityEngine.Debug.Log("[InsightServer] - Player joining MatchMaking."); }

        usersInQueue.Add(authModule.GetUserByConnection(netMsg.connectionId));
    }

    private void HandleStopMatchSearchMsg(InsightNetworkMessage netMsg)
    {
        foreach (UserContainer seraching in usersInQueue)
        {
            if (seraching.connectionId == netMsg.connectionId)
            {
                usersInQueue.Remove(seraching);
                return;
            }
        }
    }

    private void CheckQueue()
    {
        if(usersInQueue.Count >= MinimumPlayersForGame)
        {
            return;
        }

        if (masterSpawner.registeredSpawners.Count == 0)
        {
            if (server.logNetworkMessages) { UnityEngine.Debug.Log("[InsightServer] - No spawners for players in queue."); }
            return;
        }

        //Create Match
        MatchContainer newMatch = new MatchContainer();

        //Used to track completion of requested spawn
        string uniqueID = Guid.NewGuid().ToString();

        //Specify the match details
        RequestSpawn requestSpawn = new RequestSpawn()
        {
            ProcessAlias = "managedgameserver",
            SceneName = "SuperAwesomeGame",
            UniqueID = uniqueID
        };

        //Request a new server
        gameManager.RequestGameSpawn(requestSpawn);

        //Wait for it to spin up
        newMatch.MatchServer = gameManager.GetGameByUniqueID(uniqueID);

        //Add the players from the queue into this match:
        foreach (UserContainer user in usersInQueue)
        {
            newMatch.MatchUsers.Add(user);

            server.SendToClient(user.connectionId, (short)MsgId.ChangeServers, new ChangeServers() {
                NetworkAddress = newMatch.MatchServer.NetworkAddress,
                NetworkPort = newMatch.MatchServer.NetworkPort,
                SceneName = newMatch.MatchServer.SceneName
            });
        }
        usersInQueue.Clear();
    }
}

public class MatchContainer
{
    public GameContainer MatchServer;
    public List<UserContainer> MatchUsers;
}
