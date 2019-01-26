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
Note: The Spawner example requires the use of the build menu (Under Tools at the top) to have a seperate running Standalone to spawn.  

# Examples:  
1. SimpleConnection - Shows an InsightServer autostart and a InsightClient autoconnect.
2. ChatModule - Sends messages to players anywhere in the game.  
3. LoginModule - Simple user/pass verification  
4. BasicSpawner - Creates GameServer with incrementing port numbers  

# Planned Examples or WIP:  
ComplexSpawner - Create and register GameServer with GameManagerModule (almost complete)  
GameManager - Builds on ComplexSpawner to register newly spawned games to a MasterServer. (50% done)  
MatchMaking - Builds on GameManager to match players with existing games or spawn on demand. (50% done)  
Database - Working on support for MySQL and SQLite (not implemented yet)  
LoadBalancer - Track available spawners and their host health. (not implemented yet)  
AdminConsole - Simple server management from MasterServer  
