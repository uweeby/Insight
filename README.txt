Insight uses Unity and Mirror to provide a MasterServer that is seperate from the normal NetworkManager.

Insight also provides an optional set of Modules to add features on top of the MasterServer connection.

![Diagram](https://i.imgur.com/WEktiZp.png)

Built on Unity 2017.4 but should work with any newer version of Unity.

To Build:
In the Unity Editor go under Tools>Build All. Launch the MasterServer. It will create a ZoneServer. Once both are loaded open the Client.

From the client you can press 1, 2, or 3 on the keyboard to send messages to either the ZoneServer or MasterServer.

Current Modules:
SpawnerModule - Creates ZoneServers on Start and OnDemand (OnDemand not implemented yet)
ChatModule - Sends messages to players anywhere in the game. (Not fully implemented yet)
DatabaseModule - Working on support for MySQL and SQLite
ZoneModule - Manage the Zones spawned by SpawnerModule

License: Follow the included Mirror license. Otherwise Insight has no specific licensing of its own.
