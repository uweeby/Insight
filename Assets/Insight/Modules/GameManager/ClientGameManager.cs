using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Insight
{
    public class ClientGameManager : InsightModule
    {
        [HideInInspector] public InsightClient client;
        public NetworkManager networkManager;
        public TelepathyTransport transport;

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

        private void HandleChangeServersMsg(InsightNetworkMessage netMsg)
        {
            ChangeServerMsg message = netMsg.ReadMessage<ChangeServerMsg>();

            if (client.logNetworkMessages) { Debug.Log("[InsightClient] - Connecting to GameServer: " + message.NetworkAddress + ":" + message.NetworkPort + "/" + message.SceneName); }

            networkManager.networkAddress = message.NetworkAddress;
            transport.port = message.NetworkPort;
            SceneManager.LoadScene(message.SceneName);
            networkManager.StartClient();
        }

        private void HandleGameListMsg(InsightNetworkMessage netMsg)
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
        public void SendRequestSpawn(RequestSpawnMsg requestSpawnMsg)
        {
            client.Send((short)MsgId.RequestSpawn, requestSpawnMsg);
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