Insight is a Simple Message Server for Unity based on Mirror. It can operate on its network connection or alongside a NetworkMannager. Inspired by MasterServerFramework it can be expanded with flexible modules. Please see the [Wiki](https://github.com/uweenukr/Insight/wiki) for more detailed inforamtion.

### Disclaimer  
This software is currently alpha, and subject to change. Not to be used in production systems.  

### Notes:    
-Insight uses Telepathy by default for examples. All Mirror capable transports should* work.  
-Insight will use Port 7000 and Mirror (NetworkManager) will use port 7777+ for examples.  

### Requirements:  
Mirror: https://github.com/vis2k/Mirror   
Unity: 2018.2.20+  

### Examples:  
[1. SimpleConnection](https://github.com/uweenukr/Insight/wiki/Example:-1-SimpleConnection) - Shows an InsightServer autostart and a InsightClient autoconnect.
[2. ChatModule](https://github.com/uweenukr/Insight/wiki/Example:-2-Chat) - Sends messages to players anywhere in the game.  
[3. LoginModule](https://github.com/uweenukr/Insight/wiki/Example:-3-Login) - Simple user/pass verification  
[4. BasicSpawner](https://github.com/uweenukr/Insight/wiki/Example:-4-Spawner) - Creates BasicGameServer. This is a standalone NetworkManager.
[5. MasterServer](https://github.com/uweenukr/Insight/wiki/Example:-5-MasterServer) - Builds on the Spawner concept. All messages are passed through the MasterServer. RequestSpawn messages are sent to an available ChildSpawner. This ChildSpawner can be on the same machine (like the example) or any number of other machines.  
[6. GameManager](https://github.com/uweenukr/Insight/wiki/Example:-6-GameManager) -  Builds on the MasterServer example. It uses a ManagedGameServer that will register back to the GameManager module.  

### WIP Examples:  
[7. MatchMaking](https://github.com/uweenukr/Insight/wiki/Example:-WIP-7-MatchMaker) - Builds on the GameManager example. It uses a MatchMaking module to track users looking for a game. Then connects them to an available server.  
