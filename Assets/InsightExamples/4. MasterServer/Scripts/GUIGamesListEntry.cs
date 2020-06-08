using UnityEngine;
using UnityEngine.UI;

namespace Insight.Examples
{
    public class GUIGamesListEntry : MonoBehaviour
    {
        public PlayerClientGUI clientComp;

        public Text SceneNameText;
        public Text PlayerCountText;

        public string UniqueID;
        public string SceneName;
        public int CurrentPlayers;
        public int MaxPlayers;

        private bool Init;

        private void LateUpdate()
        {
            if (!Init)
            {
                Init = true;

                SceneNameText.text = SceneName;
                PlayerCountText.text = CurrentPlayers + "/" + MaxPlayers;
            }
        }

        public void HandleSelectButton()
        {
            clientComp.HandleJoinGameButton(UniqueID);
        }
    }
}
