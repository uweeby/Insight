using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public enum CallbackStatus
    {
        Ok,
        Error,
        Timeout
    }

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

        protected int callbackIdIndex = 0; // 0 is a _special_ id - it represents _no callback_. 
        protected Dictionary<int, CallbackData> callbacks = new Dictionary<int, CallbackData>();

        public delegate void CallbackHandler(CallbackStatus status, NetworkReader reader);

        public float TIMEOUTDELAY = 30f; // all callbacks have a 30 second time out. 

        public bool isConnected { get { return connectState == ConnectState.Connected; } }

        public void RegisterHandler(short msgType, InsightNetworkMessageDelegate handler)
        {
            if (messageHandlers.ContainsKey(msgType))
            {
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

        protected virtual void CheckCallbackTimeouts()
        {
            foreach (var callback in callbacks)
            {
                if (callback.Value.timeout < Time.realtimeSinceStartup)
                {
                    callback.Value.callback.Invoke(CallbackStatus.Timeout, null);
                    callbacks.Remove(callback.Key);
                    break;
                }
            }
        }

        public void ClearHandlers()
        {
            messageHandlers.Clear();
        }

        public abstract void StartInsight();

        public abstract void StopInsight();

    }
}