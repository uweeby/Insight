using Insight;
using System.Collections.Generic;
using UnityEngine;

public class LoginInsightServer : InsightServer
{
    public List<GameObject> spawnPrefabs;

    // Use this for initialization
    public override void Start ()
    {
        base.Start();

        RegisterHandlers();
    }

    // Update is called once per frame
    public override void Update ()
    {
        HandleNewMessages();
    }

    private void RegisterHandlers()
    {
        RegisterHandler(LoginMsg.MsgId, HandleLoginMsg);
    }

    private void HandleLoginMsg(InsightNetworkMessage netMsg)
    {
        print("HandleLoginMsg");

        LoginMsg message = netMsg.ReadMessage<LoginMsg>();

        print("User tried to login. Account: " + message.AccountName + " Password: " + message.AccountPassword);

        //if (true) //Do something to confirm the user/pass are correct
        //{
        //    print("Login sucessful for: " + message.AccountName);
        //}
        //else
        //{
        //    print("Login failed for: " + message.AccountName);
        //}
    }
}