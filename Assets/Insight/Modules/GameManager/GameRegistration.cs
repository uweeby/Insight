using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Insight
{
    public class GameRegistration : InsightModule
    {
        InsightClient client;
        [SerializeField] NetworkManager networkManager;
        [SerializeField] TelepathyTransport networkManagerTelepathyTransport;
        [SerializeField] TelepathyTransport insightTelepathyTransport;

        public List<GameContainer> registeredGames = new List<GameContainer>();

        //Pulled from command line arguments
        public string GameScene;
        public string NetworkAddress;
        public ushort NetworkPort;
        public string UniqueID;

        //These should probably be synced from the NetworkManager
        public int MaxPlayers;
        public int CurrentPlayers;

        public override void Initialize(InsightClient insight, ModuleManager manager)
        {
            client = insight;

            insightTelepathyTransport.OnClientConnected.AddListener(SendGameRegistrationToGameManager);

            RegisterHandlers();

            networkManager = NetworkManager.singleton;

            GatherCmdArgs();

            InvokeRepeating("SendGameStatusToGameManager", 30f, 30f);
        }

        void RegisterHandlers() { }

        void GatherCmdArgs()
        {
            InsightArgs args = new InsightArgs();
            if (args.IsProvided("-NetworkAddress"))
            {
                Debug.Log("[Args] - NetworkAddress: " + args.NetworkAddress);
                NetworkAddress = args.NetworkAddress;
            }

            if (args.IsProvided("-NetworkPort"))
            {
                Debug.Log("[Args] - NetworkPort: " + args.NetworkPort);
                NetworkPort = (ushort)args.NetworkPort;
                networkManagerTelepathyTransport.port = (ushort)args.NetworkPort;
            }

            if (args.IsProvided("-SceneName"))
            {
                Debug.Log("[Args] - SceneName: " + args.SceneName);
                GameScene = args.SceneName;
                SceneManager.LoadScene(args.SceneName);
            }

            if (args.IsProvided("-UniqueID"))
            {
                Debug.Log("[Args] - UniqueID: " + args.UniqueID);
                UniqueID = args.UniqueID;
            }

            //Start NetworkManager
            networkManager.StartServer();
        }

        void SendGameRegistrationToGameManager()
        {
            Debug.Log("[GameRegistration] - registering with master");
            client.Send((short)MsgId.RegisterGame, new RegisterGameMsg()
            {
                NetworkAddress = NetworkAddress,
                NetworkPort = NetworkPort,
                UniqueID = UniqueID,
                SceneName = GameScene,
                MaxPlayers = MaxPlayers,
                CurrentPlayers = CurrentPlayers
            });
        }

        void SendGameStatusToGameManager()
        {
            Debug.Log("[GameRegistration] - status update");
            client.Send((short)MsgId.GameStatus, new GameStatusMsg()
            {
                UniqueID = UniqueID,
                CurrentPlayers = CurrentPlayers
            });
        }
    }
}
