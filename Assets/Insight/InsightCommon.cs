using System.Collections.Generic;
using UnityEngine;

namespace Insight
{

	public abstract class InsightCommon : MonoBehaviour
	{
        public bool AutoStart = true;
        public bool logNetworkMessages = false;
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

        public abstract void StartInsight();

        public abstract void StopInsight();

        //public abstract bool Send(int connectionId, byte[] data);

        //public abstract bool SendMsg(int connectionId, short msgType, MessageBase msg);

        //public abstract bool SendMsgToAll(short msgType, MessageBase msg);
    }
}