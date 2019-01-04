using Insight;
using UnityEngine;
using UnityEngine.UI;

public class LoginGUI : MonoBehaviour
{
    public InsightClient insight;

    public InputField usernameField;
    public InputField passwordField;

    public Text statusText;

    public void HandleLoginButton()
    {
        insight.Send(LoginMsg.MsgId, new LoginMsg() { AccountName = usernameField.text, AccountPassword = passwordField.text }, (success, reader) =>
        {
            if (success == CallbackStatus.Ok)
            {
                Debug.Log("Logged in");
                StatusMsg msg = reader.ReadMessage<StatusMsg>();// new StatusMsg().Deserialize(reader);
                statusText.text = msg.Text;
            }
            else Debug.Log("login fail");
        });
    }
}