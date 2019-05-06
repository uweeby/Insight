using Insight;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerClientGUIState { Login, Main, Game};

public class PlayerClientGUI : MonoBehaviour
{
    [Header("Root UI Panels")]
    public GameObject RootLoginPanel;
    public GameObject RootMainPanel;
    public GameObject RootGamePanel;

    [HideInInspector] public PlayerClientGUIState playerGuiState;

    [Header("Insight Modules")]
    public ClientAuthentication authComp;
    public ChatClient chatComp;
    public ClientGameManager gameComp;
    public ClientMatchMaking matchComp;

    [Header("UI Buttons")]
    public GameObject StartMatchMakingButton;
    public GameObject StopMatchMakingButton;
    public GameObject GetGameListButton;
    public GameObject CreateGameButton;

    [Header("Game List UI Panels")]
    public GameObject GameListArea;
    public GameObject GameListPanel;

    public GameObject GameListItemPrefab;

    public Text chatTextField;
    public InputField chatInputField;

    public List<GameContainer> gamesList = new List<GameContainer>();

    [Header("Playlist/Game Name")]
    public string GameName = "SuperAwesomeGame";

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
                CheckGamesList();
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

        matchComp.SendStartMatchMaking(new StartMatchMakingMsg() { SceneName = GameName });
    }

    public void HandleStopMatchMakingButton()
    {
        StartMatchMakingButton.SetActive(true);
        StopMatchMakingButton.SetActive(false);

        matchComp.SendStopMatchMaking();
    }

    public void HandleGetGameListButton()
    {
        gameComp.SendGetGameListMsg();

        GetGameListButton.SetActive(false);
        StartMatchMakingButton.SetActive(false);
        StopMatchMakingButton.SetActive(false);
        CreateGameButton.SetActive(false);

        GameListArea.SetActive(true);
    }

    public void HandleJoinGameButton(string UniqueID)
    {
        gameComp.SendJoinGameMsg(UniqueID);

        playerGuiState = PlayerClientGUIState.Game;
    }

    public void HandleCancelButton()
    {
        foreach (Transform child in GameListPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        GameListArea.SetActive(false);
        GetGameListButton.SetActive(true);
        StartMatchMakingButton.SetActive(true);
        StopMatchMakingButton.SetActive(false);
        CreateGameButton.SetActive(true);
    }

    public void HandleCreateGameButton()
    {
        gameComp.SendRequestSpawnStart(new RequestSpawnStartMsg() { SceneName = GameName });
    }

    public void HandleSendChatButton()
    {
        chatComp.SendChatMsg(chatInputField.text);
        chatInputField.text = "";
    }
    
    private void CheckGamesList()
    {
        gamesList.Clear();

        if (gameComp.gamesList.Count > 0)
        {
            gamesList.AddRange(gameComp.gamesList);
            gameComp.gamesList.Clear();
            UpdateGameListUI();
        }
    }

    public void UpdateGameListUI()
    {
        foreach (GameContainer game in gamesList)
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
