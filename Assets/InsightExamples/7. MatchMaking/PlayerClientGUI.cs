using UnityEngine;

public enum PlayerClientGUIState { Login, Main, GamesList, Game};

public class PlayerClientGUI : MonoBehaviour
{
    public GameObject RootLoginPanel;
    public GameObject RootMainPanel;
    public GameObject RootGamesListPanel;

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

            case PlayerClientGUIState.GamesList:
                SwitchToGamesList();
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
        RootGamesListPanel.SetActive(false);
    }

    private void SwitchToMain()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(true);
        RootGamesListPanel.SetActive(false);
    }

    private void SwitchToGamesList()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(false);
        RootGamesListPanel.SetActive(true);
    }

    private void SwitchToGame()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(false);
        RootGamesListPanel.SetActive(false);
    }
}
