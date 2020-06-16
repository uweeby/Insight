using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public enum CallbackStatus : byte
    {
        Default,
        Success,
        Error,
        Timeout
    }

    public abstract class InsightCommon : MonoBehaviour
	{
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InsightCommon));

        public bool DontDestroy = true; //Sets DontDestroyOnLoad for this object. Default to true. Can be disabled via Inspector or runtime code.
        public bool AutoStart = true;
        public string networkAddress = "localhost";
        
        protected Dictionary<int, InsightNetworkMessageDelegate> messageHandlers = new Dictionary<int, InsightNetworkMessageDelegate>(); //Default handlers

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

        public delegate void CallbackHandler(Message netMsg);
        public delegate void SendToAllFinishedCallbackHandler(CallbackStatus status);

        public const float callbackTimeout = 30f; // all callbacks have a 30 second time out. 

        public bool isConnected { get { return connectState == ConnectState.Connected; } }

        Transport _transport;
        public virtual Transport transport
        {
            get
            {
                _transport = _transport ?? GetComponent<Transport>();
                if (_transport == null)
                    logger.LogWarning("Insight has no Transport component. Networking won't work without a Transport");
                return _transport;
            }
        }

        public void RegisterHandler<T>(Action<InsightNetworkConnection, T> handler) where T : Message, new()
        {
            int msgType = GetId<T>();
            if (messageHandlers.ContainsKey(msgType))
            {
                logger.Log("NetworkConnection.RegisterHandler replacing " + msgType);
            }
            messageHandlers[msgType] = MessageHandler(handler, false);
        }

        public void UnRegisterHandler<T>() where T : Message
        {
            int msgType = GetId<T>();
            if (messageHandlers.ContainsKey(msgType))
            {
                messageHandlers.Remove(msgType);
            }
        }

        protected virtual void CheckCallbackTimeouts()
        {
            foreach (KeyValuePair<int, CallbackData> callback in callbacks)
            {
                if (callback.Value.timeout < Time.realtimeSinceStartup)
                {
                    callback.Value.callback.Invoke(null);
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

        public struct CallbackData
        {
            public CallbackHandler callback;
            public float timeout;
        }

        [System.Serializable]
        public class SendToAllFinishedCallbackData
        {
            public SendToAllFinishedCallbackHandler callback;
            public HashSet<int> requiredCallbackIds;
            public int callbacks;
            public float timeout;
        }

        public int GetId<T>()
        {
            return typeof(T).FullName.GetStableHashCode() & 0xFFFF;
        }

        public int GetId(Type type)
        {
            return type.FullName.GetStableHashCode() & 0xFFFF;
        }

        internal static InsightNetworkMessageDelegate MessageHandler<T, C>(Action<C, T> handler, bool requireAuthenication)
            where T : Message, new()
            where C : InsightNetworkConnection
            => (conn, reader, channelId) =>
            {
                // protect against DOS attacks if attackers try to send invalid
                // data packets to crash the server/client. there are a thousand
                // ways to cause an exception in data handling:
                // - invalid headers
                // - invalid message ids
                // - invalid data causing exceptions
                // - negative ReadBytesAndSize prefixes
                // - invalid utf8 strings
                // - etc.
                //
                // let's catch them all and then disconnect that connection to avoid
                // further attacks.
                T message = default;
                try
                {
                    //if (requireAuthenication && !conn.isAuthenticated)
                    //{
                    //    // message requires authentication, but the connection was not authenticated
                    //    logger.LogWarning($"Closing connection: {conn}. Received message {typeof(T)} that required authentication, but the user has not authenticated yet");
                    //    conn.Disconnect();
                    //    return;
                    //}

                    // if it is a value type, just use defult(T)
                    // otherwise allocate a new instance
                    message = default(T) != null ? default(T) : new T();
                    message.Deserialize(reader);
                }
                catch (Exception exception)
                {
                    logger.LogError("Closed connection: " + conn + ". This can happen if the other side accidentally (or an attacker intentionally) sent invalid data. Reason: " + exception);
                    conn.Disconnect();
                    return;
                }

                handler((C)conn, message);
            };

        public bool UnpackMessage(NetworkReader messageReader, out int msgType)
        {
            // read message type (varint)
            try
            {
                msgType = messageReader.ReadUInt16();
                return true;
            }
            catch (System.IO.EndOfStreamException)
            {
                msgType = 0;
                return false;
            }
        }
    }
}
