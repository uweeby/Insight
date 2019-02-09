using UnityEngine;

public class GUIGamesListEntry : MonoBehaviour
{
    public PlayerClientGUI clientComp;
    public string UniqueID;

    public void HandleSelectButton()
    {
        clientComp.HandleJoinMatchButton(UniqueID);
    }
}
