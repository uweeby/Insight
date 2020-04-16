using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Insight
{
    public class ClientGameManager : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(MasterSpawner));

        NetworkClient client;
        [SerializeField] NetworkManager networkManager;
        [SerializeField] Transport networkManagerTransport;

        public List<GameContainer> gamesList = new List<GameContainer>();

        public override void Initialize(NetworkClient client, ModuleManager manager)
        {
            this.client = client;

            RegisterHandlers();
        }

        void RegisterHandlers()
        {
            client.Connection.RegisterHandler<ChangeServerMsg>(HandleChangeServersMsg);
            client.Connection.RegisterHandler<GameListMsg>(HandleGameListMsg);
        }

        void HandleChangeServersMsg(ChangeServerMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[InsightClient] - Connecting to GameServer: " + netMsg.NetworkAddress + ":" + netMsg.NetworkPort + "/" + netMsg.SceneName);

            if(networkManagerTransport.GetType().GetField("port") != null) {
                networkManagerTransport.GetType().GetField("port").SetValue(networkManagerTransport, netMsg.NetworkPort);
            }

            SceneManager.LoadScene(netMsg.SceneName);
            networkManager.StartClient(netMsg.NetworkAddress);
        }

        void HandleGameListMsg(GameListMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[InsightClient] - Received Games List");

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
            client.Send((short)MsgId.RequestSpawnStart, requestSpawnStartMsg);
        }

        public void SendJoinGameMsg(string UniqueID)
        {
            client.Send((short)MsgId.JoinGame, new JoinGamMsg() { UniqueID = UniqueID });
        }

        public void SendGetGameListMsg()
        {
            client.Send((short)MsgId.GameList, new GameListMsg());
        }
        #endregion
    }
}