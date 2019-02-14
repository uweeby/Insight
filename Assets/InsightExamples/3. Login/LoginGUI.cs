using Insight;
using UnityEngine;
using UnityEngine.UI;

public class LoginGUI : MonoBehaviour
{
    public ClientAuthentication clientAuthentication;

    public InputField usernameField;
    public InputField passwordField;

    public Text statusText;

    private void Update()
    {
        statusText.text = clientAuthentication.loginResponse;
    }

    //MsgSender
    public void SendLoginMsg()
    {
        clientAuthentication.SendLoginMsg(usernameField.text, passwordField.text);
    }
}