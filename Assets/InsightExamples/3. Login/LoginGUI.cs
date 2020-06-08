using UnityEngine;
using UnityEngine.UI;

namespace Insight.Examples
{
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
}
