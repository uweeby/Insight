[![Ko-Fi](https://img.shields.io/badge/Donate-Ko--Fi-red)](https://ko-fi.com/happynukegames) 
[![PayPal](https://img.shields.io/badge/Donate-PayPal-blue)](https://paypal.me/happynukegames)  

Insight is a Simple Message Server for Unity based on Mirror. It can operate on its network connection or alongside a NetworkMannager. Inspired by MasterServerFramework it can be expanded with flexible modules. Please see the [Wiki](https://github.com/uweenukr/Insight/wiki) for more detailed inforamtion.

### Requirements:  
Mirror: https://github.com/vis2k/Mirror   
Unity: 2018.3.6+  

### Versioning:  
Insight is normally built to target the current monthly/Asset Store release of Mirror. If you have any questions/issues please visit the Mirror Discord #Insight channel for support.  

### Examples:  
[1. SimpleConnection](https://github.com/uweenukr/Insight/wiki/Example:-1-SimpleConnection) - Shows an InsightServer autostart and a InsightClient autoconnect.  
[2. ChatModule](https://github.com/uweenukr/Insight/wiki/Example:-2-Chat) - Sends messages to players anywhere in the game.  
[3. LoginModule](https://github.com/uweenukr/Insight/wiki/Example:-3-Login) - Simple user/pass verification.  
[4. MasterServer](https://github.com/uweenukr/Insight/wiki/Example:-4-MasterServer) - Create games on demand or via a match maker.   

### IL2CPP Note:  
IL2CPP does not currently support System Process Spawning: https://forum.unity.com/threads/solved-il2cpp-and-process-start.533988/  
Here is a possible work around script: https://github.com/josh4364/IL2cppStartProcess/blob/master/StartExternalProcess.cs  
