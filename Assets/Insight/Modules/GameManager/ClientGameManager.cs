using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Insight
{
    public class ClientGameManager : InsightModule
    {
        InsightClient client;
        [SerializeField] NetworkManager networkManager;
        [SerializeField] TelepathyTransport networkManagerTelepathyTransport;

        public List<GameContainer> gamesList = new List<GameContainer>();

        public override void Initialize(InsightClient client, ModuleManager manager)
        {
            this.client = client;

            RegisterHandlers();
        }

        void RegisterHandlers()
        {
            client.RegisterHandler((short)MsgId.ChangeServers, HandleChangeServersMsg);
            client.RegisterHandler((short)MsgId.GameList, HandleGameListMsg);
        }

        void HandleChangeServersMsg(InsightNetworkMessage netMsg)
        {
            ChangeServerMsg message = netMsg.ReadMessage<ChangeServerMsg>();

            if (client.logNetworkMessages) { Debug.Log("[InsightClient] - Connecting to GameServer: " + message.NetworkAddress + ":" + message.NetworkPort + "/" + message.SceneName); }

            networkManager.networkAddress = message.NetworkAddress;
            networkManagerTelepathyTransport.port = message.NetworkPort;
            SceneManager.LoadScene(message.SceneName);
            networkManager.StartClient();
        }

        void HandleGameListMsg(InsightNetworkMessage netMsg)
        {
            GameListMsg message = netMsg.ReadMessage<GameListMsg>();

            if (client.logNetworkMessages) { Debug.Log("[InsightClient] - Received Games List"); };

            gamesList.Clear();

            foreach (GameContainer game in message.gamesArray)
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