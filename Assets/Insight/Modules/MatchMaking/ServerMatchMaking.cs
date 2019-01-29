using Insight;

public class ServerMatchMaking : InsightModule
{
    InsightServer server;
    ModuleManager manager;
    ServerAuthentication authModule;
    ServerGameManager gameManager;

    public void Awake()
    {
        AddDependency<ServerAuthentication>(); //Used to track logged in players
        AddDependency<ServerGameManager>(); //Used to track available games
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        this.manager = manager;
        authModule = this.manager.GetModule<ServerAuthentication>();
        gameManager = this.manager.GetModule<ServerGameManager>();
        RegisterHandlers();

        InvokeRepeating("UpdateMatches", 10f, 10f);
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.RequestMatch, HandleRequestMatchMsg);
    }

    private void HandleRequestMatchMsg(InsightNetworkMessage netMsg)
    {
        
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
