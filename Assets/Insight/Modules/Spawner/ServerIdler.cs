using Mirror;
using UnityEngine;

namespace Insight
{
    public class ServerIdler : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerIdler));

        public int MaxMinutesOfIdle;

        public override void Initialize(InsightClient insight, ModuleManager manager)
        {
            if (MaxMinutesOfIdle > 0)
            {
                InvokeRepeating("UpdateIdleState", MaxMinutesOfIdle * 60f, MaxMinutesOfIdle * 60f);
            }
        }

        void UpdateIdleState()
        {
            //Cancel if players connect to the game.
            if(NetworkManager.singleton.numPlayers > 0)
            {
                CancelInvoke();
            }

            logger.LogWarning("[ServerIdler] - No players connected within the allowed time. Shutting down server");

            NetworkManager.singleton.StopServer();

            Application.Quit();
        }
    }
}
