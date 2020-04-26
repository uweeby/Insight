using Mirror;
using UnityEngine;

namespace Insight
{
    public class InsightServer : NetworkServer
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InsightServer));

        public bool AutoStart;
        public virtual void Start()
        {
            DontDestroyOnLoad(this);
            Application.runInBackground = true;

            if (AutoStart)
            {
                Listen();
            }
        }

        public void Listen()
        {
            _ = ListenAsync();
        }

        void OnApplicationQuit()
        {
            logger.Log("[InsightServer] Stopping Server");
            Disconnect();
        }
    }
}
