using UnityEngine;
using UnityEngine.UI;

public class ChatGUI : MonoBehaviour
{
    public ChatClient chatComp;

    public InputField nameInput;
    public InputField chatInput;
    public Text textField;

    public void HandleSendChat()
    {
        chatComp.SendChatMsg(nameInput.text, chatInput.text);

        chatInput.text = ""; //Clear out the previously entered text from the field
    }
}
