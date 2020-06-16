using Mirror;
using System.Collections.Generic;

namespace Insight
{
    public class Message : MessageBase
    {
        public CallbackStatus Status = CallbackStatus.Default;
        public int callbackId { get; protected set; }
    }

    public class PropertiesMsg : Message
    {
        public string SceneName;
        public string GameType;
        public int MaxPlayers;
        public bool IsPublic;
        public string GamePassword;
    }

    public class LoginMsg : Message
    {
        public string AccountName;
        public string AccountPassword;
    }

    public class LoginResponseMsg : Message
    {
        public string UniqueID;
    }

    public class ChatMsg : Message
    {
        public short Channel; //0 for global
        public string Origin; //This could be controlled by the server.
        public string Target; //Used for private chat
        public string Data;
    }

    public class JoinChatChannelMsg : Message
    {
        public short Channel;
        public string ChannelPassword;
    }

    public class LeaveChatChannelMsg : Message
    {
        public short Channel;
    }

    //Sent from a new Spawner after it connects to a master.
    //Used to register it to the MasterSpawner
    public class RegisterSpawnerMsg : Message
    {
        public string UniqueID; //Guid
        public int MaxThreads;
    }

    //Sent from a new GameServer after it connects to a Master.
    //Used to register it to the ServerGameManager
    public class RegisterGameMsg : Message
    {
        public string UniqueID; //Guid
        public string NetworkAddress;
        public ushort NetworkPort;
        public string SceneName;
        public int MaxPlayers;
        public int CurrentPlayers;
    }

    public class GameStatusMsg : Message
    {
        public string UniqueID; //Guid
        public int CurrentPlayers;
    }

    //Sent when requesting a new GameServer to be created running the provided scene.
    public class RequestSpawnStartMsg : Message
    {
        public string SceneName;

        //Only valid in the reply from the spawner. So it should be moved to another message
        public string UniqueID; //Guid
        public string NetworkAddress; 
    }

    //Asks the server to gracefully stop
    public class RequestSpawnStopMsg : Message
    {
        public string UniqueID; //Guid
    }

    //Force kill the server. Used if the server is not responding to normal msgs.
    public class KillSpawnMsg : Message
    {
        public string UniqueID; //Guid
    }

    //Sent from a player client when they wants to join a matchmaking playlist
    public class StartMatchMakingMsg : Message
    {
        public string SceneName;
    }

    //Sent from a player client when they want to quit matchmaking
    public class StopMatchMakingMsg : Message
    {

    }

    //Sent from a player client requesting the list of currently active games.
    public class GameListMsg : Message
    {
        public GameContainer[] gamesArray;

        public void Load(List<GameContainer> gamesList)
        {
            gamesArray = gamesList.ToArray();
        }
    }

    //Sent from a palyer client when they want to join a game by its UniqueID
    public class JoinGamMsg : Message
    {
        public string UniqueID;
    }

    //Sent from the MasterServer to a player client teling them to join the listed server.
    public class ChangeServerMsg : Message
    {
        //This is msg would not support all transports in its current configuration.
        public string NetworkAddress;
        public ushort NetworkPort;
        public string SceneName;
    }

    //Updates the MasterSpawner with current status of the spawner
    //Used as a bacis load/health tracker.
    public class SpawnerStatusMsg : Message
    {
        public int CurrentThreads;
    }
}
