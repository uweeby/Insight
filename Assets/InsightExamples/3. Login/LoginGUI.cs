using UnityEngine;
using UnityEngine.UI;

public class LoginGUI : MonoBehaviour
{
    public ClientLogin loginModule;

    public InputField usernameField;
    public InputField passwordField;

    public Text statusText;

    private void Update()
    {
        statusText.text = loginModule.loginResponse;
    }
    public void HandleLoginButton()
    {
        loginModule.SendLoginMsg(usernameField.text, passwordField.text);
    }
}