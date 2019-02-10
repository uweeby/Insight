Insight is a Simple Message Server for Unity based on Mirror. It can operate on its network connection or alongside a NetworkMannager. Inspired by MasterServerFramework it can be expanded with flexible modules. Please see the [Wiki](https://github.com/uweenukr/Insight/wiki) for more detailed inforamtion.

### Disclaimer  
This software is currently alpha, and subject to change. Not to be used in production systems.  

### Requirements:  
Mirror: https://github.com/vis2k/Mirror   
Unity: 2018.2.20+  

### Examples:  
[1. SimpleConnection](https://github.com/uweenukr/Insight/wiki/Example:-1-SimpleConnection) - Shows an InsightServer autostart and a InsightClient autoconnect.  
[2. ChatModule](https://github.com/uweenukr/Insight/wiki/Example:-2-Chat) - Sends messages to players anywhere in the game.  
[3. LoginModule](https://github.com/uweenukr/Insight/wiki/Example:-3-Login) - Simple user/pass verification.  
[4. MasterServer](https://github.com/uweenukr/Insight/wiki/Example:-4-MasterServer) - Builds on the Spawner concept. All messages are passed through the MasterServer. RequestSpawn messages are sent to an available ChildSpawner. This ChildSpawner can be on the same machine (like the example) or any number of other machines.  
[5. GameManager](https://github.com/uweenukr/Insight/wiki/Example:-5-GameManager) -  Builds on the MasterServer example. It uses a ManagedGameServer that will register back to the GameManager module.  
[6. MatchMaking](https://github.com/uweenukr/Insight/wiki/Example:-6-MatchMaker) - Builds on the GameManager example. It uses a MatchMaking module to track users looking for a game. Then connects them to an available server.  
