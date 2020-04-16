using Mirror;

namespace Insight
{
    public class ClientMatchMaking : InsightModule
    {
        NetworkClient client;

        public override void Initialize(NetworkClient client, ModuleManager manager)
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
            client.Send((short)MsgId.StartMatchMaking, startMatchMakingMsg);
        }

        public void SendStopMatchMaking()
        {
            client.Send((short)MsgId.StopMatchMaking, new StopMatchMakingMsg());
        }
        #endregion
    }
}
