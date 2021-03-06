﻿using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Insight
{
    public class ClientGameManager : InsightModule
    {
        InsightClient client;
        Transport networkManagerTransport;

        public List<GameContainer> gamesList = new List<GameContainer>();

        public override void Initialize(InsightClient client, ModuleManager manager)
        {
            this.client = client;

            networkManagerTransport = Transport.activeTransport;

            RegisterHandlers();
        }

        void RegisterHandlers()
        {
            client.RegisterHandler<ChangeServerMsg>(HandleChangeServersMsg);
            client.RegisterHandler<GameListMsg>(HandleGameListMsg);
        }

        void HandleChangeServersMsg(InsightNetworkMessage netMsg)
        {
            ChangeServerMsg message = netMsg.ReadMessage<ChangeServerMsg>();

            Debug.Log("[InsightClient] - Connecting to GameServer: " + message.NetworkAddress + ":" + message.NetworkPort + "/" + message.SceneName);

            if(networkManagerTransport.GetType().GetField("port") != null) {
                networkManagerTransport.GetType().GetField("port").SetValue(networkManagerTransport, message.NetworkPort);
            }

            //For IgnoranceTransport
            if (networkManagerTransport.GetType().GetField("CommunicationPort") != null)
            {
                networkManagerTransport.GetType().GetField("CommunicationPort").SetValue(networkManagerTransport, message.NetworkPort);
            }

            NetworkManager.singleton.networkAddress = message.NetworkAddress;
            SceneManager.LoadScene(message.SceneName);
            NetworkManager.singleton.StartClient();
        }

        void HandleGameListMsg(InsightNetworkMessage netMsg)
        {
            GameListMsg message = netMsg.ReadMessage<GameListMsg>();

            Debug.Log("[InsightClient] - Received Games List");

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