using Insight;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class ClientLogin : InsightModule
{
    InsightClient client;

    public LoginGUI loginGuiComp;

    [HideInInspector]
    public string loginResponse; //This is put in the GUI. Just for example purposes

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;

        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler((short)MsgId.Status, HandleStatusMsg);
    }

    private void HandleStatusMsg(InsightNetworkMessage netMsg)
    {
        if (client.logNetworkMessages) { Debug.Log("[InsightClient] - HandleStatusMsg()"); }

        StatusMsg message = netMsg.ReadMessage<StatusMsg>();

        //Added for Demo Scene
        loginGuiComp.statusText.text = message.Text;
    }

    public void SendLoginMsg(string username, string password)
    {
        client.Send((short)MsgId.Login, new LoginMsg() { AccountName = username, AccountPassword = Sha256(password) }, (success, reader) =>
        {
            if (success == CallbackStatus.Ok)
            {
                StatusMsg msg = reader.ReadMessage<StatusMsg>();
                loginResponse = msg.Text;
                Debug.Log(msg.Text);
            }
            else Debug.Log("Callback Error: Login Message was lost or a reply was not sent");
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
