using Insight;
using System.Collections.Generic;

public class ServerMatchMaking : InsightModule
{
    InsightServer server;
    ModuleManager manager;
    ServerAuthentication authModule;
    ServerGameManager gameManager;
    MasterSpawner masterSpawner;

    public float MatchListPollRateInSeconds = 10f;

    public List<SearchingForMatch> searchingForMatch = new List<SearchingForMatch>();

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

        InvokeRepeating("UpdateMatches", MatchListPollRateInSeconds, MatchListPollRateInSeconds);
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.StartMatchMaking, HandleStartMatchSearchMsg);
        server.RegisterHandler((short)MsgId.StopMatchMaking, HandleStopMatchSearchMsg);
    }

    private void HandleStartMatchSearchMsg(InsightNetworkMessage netMsg)
    {
        StartMatchMaking message = netMsg.ReadMessage<StartMatchMaking>();

        if (server.logNetworkMessages) { UnityEngine.Debug.Log("[InsightServer] - Player joining MatchMaking."); }

        UserContainer newUser = authModule.GetUserByConnection(netMsg.connectionId);

        searchingForMatch.Add(new SearchingForMatch() { user = newUser });
    }

    private void HandleStopMatchSearchMsg(InsightNetworkMessage netMsg)
    {
        StopMatchMaking message = netMsg.ReadMessage<StopMatchMaking>();

        foreach (SearchingForMatch seraching in searchingForMatch)
        {
            if (seraching.user.connectionId == netMsg.connectionId)
            {
                searchingForMatch.Remove(seraching);
                return;
            }
        }
    }

    private void UpdateMatches()
    {
        //Check the list of players and current games at specified interval
        if(searchingForMatch.Count > 0)
        {
            if(gameManager.registeredGames.Count > 0)
            {
                //Tell the players to join the active game
                for(int i = 0; i < searchingForMatch.Count; i++)
                {
                    server.SendToClient(searchingForMatch[i].user.connectionId, (short)MsgId.ChangeServers, new ChangeServers() {
                        NetworkAddress = gameManager.registeredGames[0].NetworkAddress,
                        NetworkPort = gameManager.registeredGames[0].NetworkPort,
                        SceneName = "SuperAwesomeGame"
                    });
                    searchingForMatch.Remove(searchingForMatch[i]);
                }
            }
            else
            {
                masterSpawner.RequestGameSpawn();
            }
        }
    }

    public struct UserSeekingMatch
    {
        //Placeholder
        public string playerName;
        public string gameType;
    }
}

//Collection of users that are authenticted that are currently looking for a match
public struct SearchingForMatch
{
    public UserContainer user;
}
