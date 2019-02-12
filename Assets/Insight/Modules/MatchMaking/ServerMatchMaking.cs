using System;
using System.Collections.Generic;

namespace Insight
{
    public class ServerMatchMaking : InsightModule
    {
        public InsightServer server;
        public ModuleManager manager;
        public ServerAuthentication authModule;
        public ServerGameManager gameManager;
        public MasterSpawner masterSpawner;

        public int MinimumPlayersForGame = 1;
        public float MatchMakingPollRate = 10f;

        public List<UserContainer> playerQueue = new List<UserContainer>();
        public List<MatchContainer> matchList = new List<MatchContainer>();

        private bool _spawnInProgress;

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

            InvokeRepeating("UpdateStuff", MatchMakingPollRate, MatchMakingPollRate);
        }

        void RegisterHandlers()
        {
            server.RegisterHandler((short)MsgId.StartMatchMaking, HandleStartMatchSearchMsg);
            server.RegisterHandler((short)MsgId.StopMatchMaking, HandleStopMatchSearchMsg);
        }

        void UpdateStuff()
        {
            UpdateQueue();
            UpdateMatches();
        }

        private void HandleStartMatchSearchMsg(InsightNetworkMessage netMsg)
        {
            if (server.logNetworkMessages) { UnityEngine.Debug.Log("[MatchMaking] - Player joining MatchMaking."); }

            playerQueue.Add(authModule.GetUserByConnection(netMsg.connectionId));
        }

        private void HandleStopMatchSearchMsg(InsightNetworkMessage netMsg)
        {
            foreach (UserContainer seraching in playerQueue)
            {
                if (seraching.connectionId == netMsg.connectionId)
                {
                    playerQueue.Remove(seraching);
                    return;
                }
            }
        }

        private void UpdateQueue()
        {
            if (playerQueue.Count < MinimumPlayersForGame)
            {
                if (server.logNetworkMessages) { UnityEngine.Debug.Log("[MatchMaking] - Minimum players in queue not reached."); }
                return;
            }

            if (masterSpawner.registeredSpawners.Count == 0)
            {
                if (server.logNetworkMessages) { UnityEngine.Debug.Log("[MatchMaking] - No spawners for players in queue."); }
                return;
            }

            CreateMatch();
        }

        private void CreateMatch()
        {
            //Used to track completion of requested spawn
            string uniqueID = Guid.NewGuid().ToString();

            //Specify the match details
            RequestSpawnMsg requestSpawn = new RequestSpawnMsg()
            {
                //This should not be hard coded. Where should it go?
                ProcessAlias = "gameserver",
                SceneName = "SuperAwesomeGame",
                UniqueID = uniqueID
            };

            List<UserContainer> matchUsers = new List<UserContainer>();

            //This should check to make sure that the max players is not higher than the number in queue
            //Add the players from the queue into this match:
            for(int i = playerQueue.Count -1; i >= 0; i--)
            {
                matchUsers.Add(playerQueue[i]);
                playerQueue.RemoveAt(i);
            }

            matchList.Add(new MatchContainer(this, requestSpawn, matchUsers));
        }

        private void UpdateMatches()
        {
            foreach (MatchContainer match in matchList)
            {
                if(match.MatchComplete)
                {
                    matchList.Remove(match);
                }
                match.Update();
            }
        }
    }

    public class MatchContainer
    {
        public ServerMatchMaking matchModule;
        public GameContainer MatchServer;
        public List<UserContainer> matchUsers;

        //These two are probably redundant
        public string playlistName;
        public RequestSpawnMsg matchProperties;

        public DateTime matchStartTime;
        public float MatchTimeoutInSeconds = 30f; //How long to wait for the server to start before cancelling the match and returning the players to the queue

        public bool InitMatch;
        public bool MatchComplete;

        public MatchContainer(ServerMatchMaking MatchModule, RequestSpawnMsg MatchProperties, List<UserContainer> MatchUsers)
        {
            matchModule = MatchModule;
            matchProperties = MatchProperties;
            matchModule.gameManager.RequestGameSpawn(matchProperties);
            matchUsers = MatchUsers;
            matchStartTime = DateTime.UtcNow;
        }

        public void Update()
        {
            if(!InitMatch)
            {
                //Check to see if the server is up
                if (matchModule.gameManager.GetGameByUniqueID(matchProperties.UniqueID) == null)
                {
                    //Server spawn timeout check
                    if(matchStartTime.AddSeconds(MatchTimeoutInSeconds) < DateTime.UtcNow)
                    {
                        CancelMatch();
                    }

                    UnityEngine.Debug.Log("Server not active at this time");
                    return;
                }
                //Server is registered and active
                else
                {
                    InitMatch = true;
                    MatchServer = matchModule.gameManager.GetGameByUniqueID(matchProperties.UniqueID);

                    //Move players to server
                    foreach (UserContainer user in matchUsers)
                    {
                        matchModule.server.SendToClient(user.connectionId, (short)MsgId.ChangeServers, new ChangeServerMsg()
                        {
                            NetworkAddress = MatchServer.NetworkAddress,
                            NetworkPort = MatchServer.NetworkPort,
                            SceneName = MatchServer.SceneName
                        });
                    }
                }
            }
        }

        private void CancelMatch()
        {
            UnityEngine.Debug.LogError("Server failed to start within timoue period. Cancelling match.");

            //TODO: Destroy the match process somewhere: MatchServer

            //Put the users back in the queue
            foreach (UserContainer user in matchUsers)
            {
                matchModule.playerQueue.Add(user);
            }
            matchUsers.Clear();

            //Flag to destroy match on next update
            MatchComplete = true;
        }
    }
}
