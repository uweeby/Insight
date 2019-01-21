using UnityEngine;

public class MasterServerGUI : MonoBehaviour
{
    public GameObject PlayerGameObject;

    //This is just a quick way to give the Master/Spawner some time to start up. Its only for the example scene.
    public void HandlePlayerLoginButton()
    {
        PlayerGameObject.SetActive(true);
    }
}
