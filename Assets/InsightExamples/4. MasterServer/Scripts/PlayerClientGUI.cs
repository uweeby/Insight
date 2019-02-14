using Insight;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerClientGUIState { Login, Main, Game};

public class PlayerClientGUI : MonoBehaviour
{
    public GameObject RootLoginPanel;
    public GameObject RootMainPanel;
    public GameObject RootGamePanel;

    public PlayerClientGUIState playerGuiState;

    public ClientAuthentication authComp;
    public ChatClient chatComp;
    public ClientMatchMaking matchComp;

    public GameObject StartMatchMakingButton;
    public GameObject StopMatchMakingButton;
    public GameObject GetGameListButton;
    public GameObject CreateGameButton;

    public GameObject GameListArea;
    public GameObject GameListPanel;

    public GameObject GameListItemPrefab;

    public Text chatTextField;

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

    public void FixedUpdate()
    {
        //This is gross. Needs a better design that does not introduce coupling.
        chatTextField.text = chatComp.chatLog;
    }

    private void SwitchToLogin()
    {
        RootLoginPanel.SetActive(true);
        RootMainPanel.SetActive(false);
        RootGamePanel.SetActive(false);
    }

    private void SwitchToMain()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(true);
        RootGamePanel.SetActive(false);
    }

    private void SwitchToGamesList()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(false);
        RootGamePanel.SetActive(false);
    }

    private void SwitchToGame()
    {
        RootLoginPanel.SetActive(false);
        RootMainPanel.SetActive(false);
        RootGamePanel.SetActive(true);
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

    public void HandleGetGameListButton()
    {
        matchComp.SendGetGameListMsg();

        GetGameListButton.SetActive(false);
        StartMatchMakingButton.SetActive(false);
        StopMatchMakingButton.SetActive(false);

        GameListArea.SetActive(true);
    }

    public void HandleJoinGameButton(string UniqueID)
    {
        matchComp.SendJoinGameMsg(UniqueID);

        playerGuiState = PlayerClientGUIState.Game;
    }

    public void HandleCancelButton()
    {
        GameListArea.SetActive(false);

        GetGameListButton.SetActive(true);
        StartMatchMakingButton.SetActive(true);
        StopMatchMakingButton.SetActive(false);
    }

    public void HandleCreateGameButton()
    {

        matchComp.SendRequestSpawn();
    }

    public void HandleSendChatButton(string Data)
    {
        chatComp.SendChatMsg(Data);
    }

    public void UpdateGameListUI()
    {
        foreach (GameContainer game in matchComp.gamesList)
        {
            GameObject instance = Instantiate(GameListItemPrefab);
            instance.transform.parent = GameListPanel.transform;
            GUIGamesListEntry comp = instance.GetComponent<GUIGamesListEntry>();
            comp.clientComp = this;
            comp.UniqueID = game.UniqueId;
            comp.CurrentPlayers = game.CurrentPlayers;
            comp.MaxPlayers = game.MaxPlayers;
            comp.SceneName = game.SceneName;
        }
    }
}
