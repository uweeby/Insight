using Insight;
using System.Collections.Generic;

public class ServerMatchMaking : InsightModule
{
    InsightServer server;
    ModuleManager manager;
    ServerAuthentication authModule;
    ServerGameManager gameManager;
    MasterSpawner masterSpawner;

    public int MinimumPlayersForGame;

    public List<UserContainer> usersInQueue = new List<UserContainer>();

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

        usersInQueue.Add(authModule.GetUserByConnection(netMsg.connectionId));

        CheckQueue();
    }

    private void HandleStopMatchSearchMsg(InsightNetworkMessage netMsg)
    {
        StopMatchMaking message = netMsg.ReadMessage<StopMatchMaking>();

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
        //Check the list of players and current games at specified interval
        if(usersInQueue.Count > MinimumPlayersForGame)
        {
            if(gameManager.registeredGameServers.Count > 0)
            {
                //Tell the players to join the active game
                for(int i = 0; i < usersInQueue.Count; i++)
                {
                    server.SendToClient(usersInQueue[i].connectionId, (short)MsgId.ChangeServers, new ChangeServers() {
                        NetworkAddress = gameManager.registeredGameServers[0].NetworkAddress,
                        NetworkPort = gameManager.registeredGameServers[0].NetworkPort,
                        SceneName = "SuperAwesomeGame"
                    });
                    usersInQueue.Remove(usersInQueue[i]);
                }
            }
            else
            {
                masterSpawner.RequestGameSpawn();
            }
        }
    }
}
