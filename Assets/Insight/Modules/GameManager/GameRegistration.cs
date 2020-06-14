using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Insight
{
    public class GameRegistration : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(GameRegistration));

        InsightClient client;
        [SerializeField] Transport networkManagerTransport;
        [SerializeField] Transport insightTransport;

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

            insightTransport.OnClientConnected.AddListener(SendGameRegistrationToGameManager);

            RegisterHandlers();

            GatherCmdArgs();

            InvokeRepeating("SendGameStatusToGameManager", 30f, 30f);
        }

        void RegisterHandlers() { }

        void GatherCmdArgs()
        {
            InsightArgs args = new InsightArgs();
            if (args.IsProvided("-NetworkAddress"))
            {
                logger.Log("[Args] - NetworkAddress: " + args.NetworkAddress);
                NetworkAddress = args.NetworkAddress;
            }

            if (args.IsProvided("-NetworkPort"))
            {
                logger.Log("[Args] - NetworkPort: " + args.NetworkPort);
                NetworkPort = (ushort)args.NetworkPort;

                if(networkManagerTransport.GetType().GetField("port") != null) {
                    networkManagerTransport.GetType().GetField("port").SetValue(networkManagerTransport, (ushort)args.NetworkPort);
                }

                //For IgnoranceTransport
                if (networkManagerTransport.GetType().GetField("CommunicationPort") != null)
                {
                    networkManagerTransport.GetType().GetField("CommunicationPort").SetValue(networkManagerTransport, (ushort)args.NetworkPort);
                }
            }

            if (args.IsProvided("-SceneName"))
            {
                logger.Log("[Args] - SceneName: " + args.SceneName);
                GameScene = args.SceneName;
                SceneManager.LoadScene(args.SceneName);
            }

            if (args.IsProvided("-UniqueID"))
            {
                logger.Log("[Args] - UniqueID: " + args.UniqueID);
                UniqueID = args.UniqueID;
            }

            MaxPlayers = NetworkManager.singleton.maxConnections;

            //Start NetworkManager
            NetworkManager.singleton.StartServer();
        }

        void SendGameRegistrationToGameManager()
        {
            logger.Log("[GameRegistration] - registering with master");
            client.Send(new RegisterGameMsg()
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
            //Update with current values from NetworkManager:
            CurrentPlayers = NetworkManager.singleton.numPlayers;

            logger.Log("[GameRegistration] - status update");
            client.Send(new GameStatusMsg()
            {
                UniqueID = UniqueID,
                CurrentPlayers = CurrentPlayers
            });
        }
    }
}
