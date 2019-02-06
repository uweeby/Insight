using Insight;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRegistration : InsightModule
{
    InsightClient client;
    public NetworkManager networkManager;
    public TelepathyTransport telepathyTransport;

    public List<GameContainer> registeredGames = new List<GameContainer>();

    //Pulled from command line arguments
    public string GameScene;
    public string NetworkAddress;
    public ushort NetworkPort;
    public string UniqueID;

    public override void Initialize(InsightClient insight, ModuleManager manager)
    {
        client = insight;

        telepathyTransport.OnClientConnected.AddListener(SendGameRegistrationToGameManager);

        RegisterHandlers();

        networkManager = NetworkManager.singleton;

        GatherCmdArgs();
    }

    void RegisterHandlers()
    {
    }

    private void GatherCmdArgs()
    {
        InsightArgs args = new InsightArgs();
        if (args.IsProvided("-AssignedPort"))
        {
            Debug.Log("Setting Network Port based on Args provided: " + args.AssignedPort);
            NetworkPort = (ushort)args.AssignedPort;
            telepathyTransport.port = (ushort)args.AssignedPort;
        }

        if (args.IsProvided("-SceneName"))
        {
            Debug.Log("Loading Scene: " + args.SceneName);
            GameScene = args.SceneName;
            SceneManager.LoadScene(args.SceneName);
        }

        if (args.IsProvided("-UniqueID"))
        {
            Debug.Log("Loading Scene: " + args.UniqueID);
            UniqueID = args.UniqueID;
        }

        //Start NetworkManager
        networkManager.StartServer();
    }

    private void SendGameRegistrationToGameManager()
    {
        Debug.Log("sending registration msg back to master");
        client.Send((short)MsgId.RegisterGame, new RegisterGameMsg() { UniqueID = Guid.NewGuid().ToString()});
    }
}
