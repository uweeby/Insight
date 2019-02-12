using Insight;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class ClientAuthentication : InsightModule
{
    InsightClient client;

    public LoginGUI loginGuiComp;

    [HideInInspector]
    public string loginResponse; //This is put in the GUI. Just for example purposes
    [HideInInspector]
    public bool loginSucessful;
    public string uniqueID;

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
        client.Send((short)MsgId.Login, new LoginMsg() { AccountName = username, AccountPassword = Sha256(password) }, (callbackStatus, reader) =>
        {
            if (callbackStatus == CallbackStatus.Ok)
            {
                LoginResponseMsg msg = reader.ReadMessage<LoginResponseMsg>();
                loginSucessful = msg.Authenticated; //This will always be true for prototyping
                if(loginSucessful)
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

    private string Sha256(string input)
    {
        var crypt = new SHA256Managed();
        var hash = new StringBuilder();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(input));
        foreach (byte theByte in crypto)
        {
            hash.Append(theByte.ToString("x2"));
        }
        return hash.ToString();
    }
}
