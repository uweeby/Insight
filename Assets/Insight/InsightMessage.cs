using Mirror;
using System.Collections.Generic;

namespace Insight
{
    public class ErrorMsg : MessageBase
    {
        public string Text;
        public bool CauseDisconnect;
    }

    public class EmptyMsg : MessageBase { }

    public class StatusMsg : MessageBase
    {
        public string Text;
    }

    public class PropertiesMsg : MessageBase
    {
        public string SceneName;
        public string GameType;
        public int MaxPlayers;
        public bool IsPublic;
        public string GamePassword;
    }

    public class LoginMsg : MessageBase
    {
        public string AccountName;
        public string AccountPassword;
    }

    public class LoginResponseMsg : MessageBase
    {
        public bool Authenticated;
        public string UniqueID;
    }

    public class ChatMsg : MessageBase
    {
        public short Channel; //0 for global
        public string Origin; //This could be controlled by the server.
        public string Target; //Used for private chat
        public string Data;
    }

    public class JoinChatChannelMsg : MessageBase
    {
        public short Channel;
        public string ChannelPassword;
    }

    public class LeaveChatChannelMsg : MessageBase
    {
        public short Channel;
    }

    //Sent from a new Spawner after it connects to a master.
    //Used to register it to the MasterSpawner
    public class RegisterSpawnerMsg : MessageBase
    {
        public string UniqueID; //Guid
        public int MaxThreads;
    }

    //Sent from a new GameServer after it connects to a Master.
    //Used to register it to the ServerGameManager
    public class RegisterGameMsg : MessageBase
    {
        public string UniqueID; //Guid
        public string NetworkAddress;
        public ushort NetworkPort;
        public string SceneName;
        public int MaxPlayers;
        public int CurrentPlayers;
    }

    public class GameStatusMsg : MessageBase
    {
        public string UniqueID; //Guid
        public int CurrentPlayers;
    }

    //Sent when requesting a new GameServer to be created running the provided scene.
    public class RequestSpawnStartMsg : MessageBase
    {
        public string SceneName;

        //Only valid in the reply from the spawner. So it should be moved to another message
        public string UniqueID; //Guid
        public string NetworkAddress; 
    }

    //Asks the server to gracefully stop
    public class RequestSpawnStopMsg : MessageBase
    {
        public string UniqueID; //Guid
    }

    //Force kill the server. Used if the server is not responding to normal msgs.
    public class KillSpawnMsg : MessageBase
    {
        public string UniqueID; //Guid
    }

    //Sent from a player client when they wants to join a matchmaking playlist
    public class StartMatchMakingMsg : MessageBase
    {
        public string SceneName;
    }

    //Sent from a player client when they want to quit matchmaking
    public class StopMatchMakingMsg : MessageBase
    {

    }

    //Sent from a player client requesting the list of currently active games.
    public class GameListMsg : MessageBase
    {
        public GameContainer[] gamesArray;

        public void Load(Dictionary<INetworkConnection, GameContainer> gamesList)
        {
            gamesArray = (new List<GameContainer>(gamesList.Values)).ToArray();
        }
    }

    //Sent from a palyer client when they want to join a game by its UniqueID
    public class JoinGameMsg : MessageBase
    {
        public string UniqueID;
    }

    //Sent from the MasterServer to a player client teling them to join the listed server.
    public class ChangeServerMsg : MessageBase
    {
        //This is msg would not support all transports in its current configuration.
        public string NetworkAddress;
        public ushort NetworkPort;
        public string SceneName;
    }

    //Updates the MasterSpawner with current status of the spawner
    //Used as a bacis load/health tracker.
    public class SpawnerStatusMsg : MessageBase
    {
        public int CurrentThreads;
    }
}
