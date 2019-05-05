using Insight;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GUIMasterServer : MonoBehaviour
{
    [Header("Insight")]
    public InsightServer masterServer;
    public ModuleManager moduleManager;
    public ServerAuthentication authModule;
    public ChatServer chatModule;
    public MasterSpawner masterSpawnerModule;
    public ServerGameManager gameModule;
    public ServerMatchMaking matchModule;

    [Header("Labels")]
    public Text spawnerCountText;
    public Text gameCountText;
    public Text userCountText;
    public Text playersInQueueCountText;
    public Text activeGamesText;

    bool Init;

    void FixedUpdate()
    {
        if(!Init)
        {
            if(masterServer.isConnected)
            {
                Init = true;
                moduleManager = masterServer.GetComponent<ModuleManager>();
                authModule = moduleManager.GetModule<ServerAuthentication>();
                chatModule = moduleManager.GetModule<ChatServer>();
                masterSpawnerModule = moduleManager.GetModule<MasterSpawner>();
                gameModule = moduleManager.GetModule<ServerGameManager>();
                matchModule = moduleManager.GetModule<ServerMatchMaking>();
            }
            return;
        }

        spawnerCountText.text = masterSpawnerModule.registeredSpawners.Count.ToString();
        gameCountText.text = gameModule.registeredGameServers.Count.ToString();
        userCountText.text = authModule.registeredUsers.Count.ToString();
        playersInQueueCountText.text = matchModule.playerQueue.Count.ToString();

        //Clear previous values
        activeGamesText.text = "";

        //Game Status
        foreach (GameContainer game in gameModule.registeredGameServers)
        {
            activeGamesText.text += game.UniqueId + " - " + game.NetworkAddress + ":" + game.NetworkPort + " - " + game.SceneName + " - " + game.CurrentPlayers + "/" + game.MaxPlayers + Environment.NewLine;
        }
    }
}
