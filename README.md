# Insight  
Insight is a simple Socket Server for Unity. It uses Mirror to create a simple net connection that can run at the same time as a NetworkManager. It can also be its own standalone network connection. The original goal of this project was to create a secondary connection to allow a MasterServer. It can be used for that or many other things. The InsightServer can run on its own or with modules.

# Disclaimer  
This software is currently alpha, and subject to change. Not to be used in production systems.  

# Requirements:  
Mirror - Download from the Asset Store or Github: https://github.com/vis2k/Mirror  
Unity - Targeting 2017.4 currently. WIP 2018 Branch: https://github.com/uweenukr/Insight/tree/2018  

You can run both the Client and Server from the same exe but the scope of this project expects that you would have a seperate Client and Server. You could use the modules to provide different services on different server processes.  

# To Build:  
Download this project and open in Unity.  
Download Mirror from one of the two locations listed above.  
Run the example scenes in editor.  

Notes:  
-The Spawner example requires the use of the build menu (Under Tools at the top) to have a seperate running Standalone to spawn.  
-Insight Examples are setup to use Telepathy by default. By convention Insight will use Port 7000 and Mirror (NetworkManager) will use port 7777+.  


# Working Examples:  
1. SimpleConnection - Shows an InsightServer autostart and a InsightClient autoconnect.
2. ChatModule - Sends messages to players anywhere in the game.  
3. LoginModule - Simple user/pass verification  
4. BasicSpawner - Creates BasicGameServer. This is a standalone NetworkManager.
5. MasterServer - Builds on the Spawner concept. All messages are passed through the MasterServer. RequestSpawn messages are sent to an available ChildSpawner. This ChildSpawner can be on the same machine (like the example) or any number of other machines.  
6. GameManager -  Builds on the MasterServer example. It uses a ManagedGameServer that will register back to the GameManager module.  

# WIP Examples:  
7. MatchMaking - Builds on the GameManager example. It uses a MatchMaking module to track users looking for a game. Then connects them to an available server.  
