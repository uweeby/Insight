using UnityEngine;

public enum PlayerClientGUIState { Login, Main, Game};

public class PlayerClientGUI : MonoBehaviour
{
    public GameObject RootLoginPanel;
    public GameObject RootMainPanel;

    public PlayerClientGUIState playerGuiState;

    public ClientAuthentication authComp;
    public ClientMatchMaking matchComp;

    public GameObject StartMatchMakingButton;
    public GameObject StopMatchMakingButton;
    public GameObject GetMatchListButton;
    public GameObject MatchListArea;

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

                if(authComp.loginSucessful)
                {
                    playerGuiState = PlayerClientGUIState.Main;
                    return;
                }
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

    public void HandleStartMatchMakingButton()
    {
        StartMatchMakingButton.SetActive(false);
        StopMatchMakingButton.SetActive(true);

        matchComp.SendStartMatchMaking();
    }

    public void HandleStopMatchMakingButton()
    {
        StartMatchMakingButton.SetActive(true);
        StopMatchMakingButton.SetActive(false);

        matchComp.SendStopMatchMaking();
    }

    public void HandleGetMatchListButton()
    {
        GetMatchListButton.SetActive(false);
        StartMatchMakingButton.SetActive(false);
        StopMatchMakingButton.SetActive(false);

        MatchListArea.SetActive(true);
    }

    public void HandleJoinMatchButton(string UniqueID)
    {
        matchComp.SendJoinMatch(UniqueID);
    }
}
