using Insight;
using UnityEngine;
using UnityEngine.UI;

public class LoginGUI : MonoBehaviour
{
    public InsightCommon insight;

    public InputField usernameField;
    public InputField passwordField;

    public Text statusText;

    public void HandleLoginButton()
    {
        insight.SendMsg(0, LoginMsg.MsgId, new LoginMsg() { AccountName = usernameField.text, AccountPassword = passwordField.text });
    }
}