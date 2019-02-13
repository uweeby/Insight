using UnityEngine;

//TODO: Remove the example specific code from module

namespace Insight
{
    public class ClientAuthentication : InsightModule
    {
        [HideInInspector] public InsightClient client;

        public LoginGUI loginGuiComp;

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
            client.Send((short)MsgId.Login, new LoginMsg() { AccountName = username, AccountPassword = password }, (callbackStatus, reader) =>
            {
                if (callbackStatus == CallbackStatus.Ok)
                {
                    LoginResponseMsg msg = reader.ReadMessage<LoginResponseMsg>();
                    loginSucessful = msg.Authenticated; //This will always be true for prototyping
                if (loginSucessful)
                    {
                        uniqueID = msg.UniqueID;
                        loginResponse = "Login Successful!";
                    }
                    else
                    {
                        loginResponse = "Login Failed!";
                    }
                }
                if (callbackStatus == CallbackStatus.Error)
                {
                    Debug.LogError("Callback Error: Login error");
                }
                if (callbackStatus == CallbackStatus.Timeout)
                {
                    Debug.LogError("Callback Error: Login attempt timed out");
                }
            });
        }
    }
}