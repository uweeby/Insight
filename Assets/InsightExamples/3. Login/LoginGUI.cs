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
    public void HandleLoginButton()
    {
        clientAuthentication.SendLoginMsg(usernameField.text, passwordField.text);
    }
}