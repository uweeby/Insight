using UnityEngine;

public enum PlayerClientGUIState { Login, Main, Game};

public class PlayerClientGUI : MonoBehaviour
{
    public GameObject RootLoginPanel;
    public GameObject RootMainPanel;

    public PlayerClientGUIState playerGuiState;

    private void Start()
    {
        SwitchToLogin();
    }

    void Update()
    {
        switch (playerGuiState)
        {
            case PlayerClientGUIState.Login:
                SwitchToLogin();
                break;
            case PlayerClientGUIState.Main:
                SwitchToMain();
                break;
            case PlayerClientGUIState.Game:
                SwitchToGame();
                break;
        }
    }

    private void SwitchToLogin()
    {
        RootLoginPanel.SetActive(true);
        RootMainPanel.SetActive(false);
    }

    private void SwitchToMain()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(true);
    }

    private void SwitchToGamesList()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(false);
    }

    private void SwitchToGame()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(false);
    }

    //Msg Senders
    public void SendFindMatch()
    {

    }
}
