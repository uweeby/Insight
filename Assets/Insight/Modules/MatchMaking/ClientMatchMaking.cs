using UnityEngine;

namespace Insight
{
    public class ClientMatchMaking : InsightModule
    {
        InsightClient client;

        public override void Initialize(InsightClient client, ModuleManager manager)
        {
            this.client = client;

            RegisterHandlers();
        }

        void RegisterHandlers()
        {

        }

        #region Message Senders
        public void SendStartMatchMaking(StartMatchMakingMsg startMatchMakingMsg)
        {
            client.Send(startMatchMakingMsg);
        }

        public void SendStopMatchMaking()
        {
            client.Send(new StopMatchMakingMsg());
        }
        #endregion
    }
}
