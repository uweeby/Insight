using Insight;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameManager : InsightModule
{
    InsightServer server;
    MasterSpawner masterSpawner;

    public List<GameContainer> registeredGames = new List<GameContainer>();

    public void Awake()
    {
        AddDependency<MasterSpawner>();
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        masterSpawner = manager.GetModule<MasterSpawner>();
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.RegisterGame, HandleRegisterGameMsg);
    }

    private void HandleRegisterGameMsg(InsightNetworkMessage netMsg)
    {
        RegisterGameMsg message = netMsg.ReadMessage<RegisterGameMsg>();

        if (server.logNetworkMessages) { Debug.Log("Received GameRegistration request"); }

        registeredGames.Add(new GameContainer() { connectionId = netMsg.connectionId, uniqueId = message.UniqueID });
    }
}

public partial class MasterSpawner
{
    public void RequestGameSpawn() //Take in the options here
    {

    }
}

public struct GameContainer
{
    public string uniqueId;
    public int connectionId;
    public Dictionary<string, string> Properties;
}
