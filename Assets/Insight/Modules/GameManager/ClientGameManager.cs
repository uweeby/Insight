using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Insight
{
    public class ClientGameManager : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientGameManager));

        InsightClient client;
        [SerializeField] NetworkManager networkManager;
        [SerializeField] AsyncTransport networkManagerTransport;

        public List<GameContainer> gamesList = new List<GameContainer>();

        public override void Initialize(InsightClient client, ModuleManager manager)
        {
            this.client = client;

            client.Authenticated.AddListener(RegisterHandlers);
        }

        void RegisterHandlers(INetworkConnection conn)
        {
            conn.RegisterHandler<ChangeServerMsg>(HandleChangeServersMsg);
            conn.RegisterHandler<GameListMsg>(HandleGameListMsg);
        }

        void HandleChangeServersMsg(ChangeServerMsg netMsg)
        {
            logger.Log("[InsightClient] - Connecting to GameServer: " + netMsg.NetworkAddress + ":" + netMsg.NetworkPort + "/" + netMsg.SceneName);

            if(networkManagerTransport.GetType().GetField("port") != null) {
                networkManagerTransport.GetType().GetField("port").SetValue(networkManagerTransport, netMsg.NetworkPort);
            }

            SceneManager.LoadScene(netMsg.SceneName);
            networkManager.StartClient(netMsg.NetworkAddress);
        }

        void HandleGameListMsg(GameListMsg netMsg)
        {
            logger.Log("[InsightClient] - Received Games List");

            gamesList.Clear();

            foreach (GameContainer game in netMsg.gamesArray)
            {
                Debug.Log(game.SceneName);

                gamesList.Add(new GameContainer()
                {
                    UniqueId = game.UniqueId,
                    SceneName = game.SceneName,
                    CurrentPlayers = game.CurrentPlayers,
                    MaxPlayers = game.MaxPlayers,
                    MinPlayers = game.MinPlayers
                });
            }
        }

        #region Message Senders
        public void SendRequestSpawnStart(RequestSpawnStartMsg requestSpawnStartMsg)
        {
            client.Send(requestSpawnStartMsg);
        }

        public void SendJoinGameMsg(string UniqueID)
        {
            client.Send(new JoinGameMsg() { UniqueID = UniqueID });
        }

        public void SendGetGameListMsg()
        {
            client.Send(new GameListMsg());
        }
        #endregion
    }
}