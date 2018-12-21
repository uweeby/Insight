using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
	public class InsightCommon : MonoBehaviour
	{
        public bool AutoStart;
        public string networkAddress = "localhost";
        public int networkPort = 5000;

        protected Dictionary<short, InsightNetworkMessageDelegate> messageHandlers; //Default Handlers

		protected enum ConnectState
        {
            None,
            Connecting,
            Connected,
            Disconnected,
        }
        protected ConnectState connectState = ConnectState.None;

        public bool isConnected { get { return connectState == ConnectState.Connected; } }

        public void RegisterHandler(short msgType, InsightNetworkMessageDelegate handler)
        {
            if (messageHandlers.ContainsKey(msgType))
            {
                //if (LogFilter.Debug) { Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType); }
                Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType);
            }
            messageHandlers[msgType] = handler;
        }

        public void UnRegisterHandler(short msgType, InsightNetworkMessageDelegate handler)
        {
            if (messageHandlers.ContainsKey(msgType))
            {
                messageHandlers[msgType] -= handler;
            }
        }

        public void ClearHandlers()
        {
            messageHandlers.Clear();
        }
    }
}