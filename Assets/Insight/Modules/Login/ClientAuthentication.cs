using Mirror;
using UnityEngine;

//TODO: Remove the example specific code from module

namespace Insight
{
    public class ClientAuthentication : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ClientAuthentication));

        InsightClient client;

        public string uniqueID;

        //This is put in the GUI. Just for example purposes
        [HideInInspector] public string loginResponse;
        [HideInInspector] public bool loginSucessful;

        public override void Initialize(InsightClient client, ModuleManager manager)
        {
            this.client = client;

            RegisterHandlers();
        }

        void RegisterHandlers()
        {

        }

        public void SendLoginMsg(string username, string password)
        {
            client.Send(new LoginMsg() { AccountName = username, AccountPassword = password }, (reader) =>
            {
                LoginResponseMsg msg = reader.ReadMessage<LoginResponseMsg>();

                if (msg.Status == CallbackStatus.Success)
                {
                    uniqueID = msg.UniqueID;
                    loginSucessful = true;
                    loginResponse = "Login Successful!";
                    logger.Log("[ClientAuthentication] - Login Successful!");
                }
                if (msg.Status == CallbackStatus.Error)
                {
                    logger.LogError("[ClientAuthentication] - Callback Error: Login error");
                }
                if (msg.Status == CallbackStatus.Timeout)
                {
                    logger.LogError("[ClientAuthentication] - Callback Error: Login attempt timed out");
                }
            });
        }
    }
}