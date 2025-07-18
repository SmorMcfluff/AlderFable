using TMPro;
using UnityEngine;

public class Chatting : MonoBehaviour
{
    public ChatBubble chatBubble;
    public bool isTyping;
    public TMP_InputField chatField;

    private void Awake()
    {
        chatBubble.owner = transform;
    }

    public void ToggleChat()
    {
        chatField.gameObject.SetActive(!chatField.isActiveAndEnabled);
        isTyping = chatField.isActiveAndEnabled;

        if (isTyping)
        {
            chatField.Select();
            chatField.ActivateInputField();
        }
    }

    public void SendChat()
    {
        if (chatField.text.Trim() != string.Empty)
        {
            chatBubble.SendChat(chatField.text);
            chatField.text = string.Empty;
        }
        ToggleChat();
    }
}
