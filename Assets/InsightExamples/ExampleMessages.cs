using Mirror;
    
//Test msg for SpawnerExample
public class SpawnRequest : MessageBase
{
    public static short MsgId = 1011;
    public string GameName; //or SceneName
    public string UniqueID;
    public string NetworkAddress;
    public int NetworkPort;
}

//Test msg for GameManager
public class GamesList : MessageBase
{
    public static short MsgId = 1014;
    //OptionCollection GOES HERE
}

//Test msg for MatchMaker
public class FindMatch : MessageBase
{
    public static short MsgId = 1015;

    //options/param of Match they want to join
}
