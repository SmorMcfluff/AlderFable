using TMPro;
using UnityEngine;

public class Chatting : MonoBehaviour
{
    public ChatBubble chatBubble;
    public bool isTyping;
    public TMP_InputField chatField;
    public string username;

    private void Awake()
    {
        if (chatBubble == null)
        {
            chatBubble = GameObject.Find("YellowDudeChatBubble").GetComponent<ChatBubble>();
        }
        chatBubble.owner = transform;
    }

    private void Start()
    {
        if (GetComponent<PlayerInput>())
        {
            username = GameManager.Instance.playerUsername;
        }
        else
        {
            int random = Random.Range(0, GameManager.Instance.friendNicknames.Length);
            username = GameManager.Instance.friendNicknames[random];
        }
    }

    public void ToggleChat()
    {
        if (chatField == null) return;
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
            chatBubble.SendChat(username, chatField.text);
            chatField.text = string.Empty;
        }
        ToggleChat();
    }

    public void SendChat(string message)
    {
        chatBubble.SendChat(username, message);
    }
}
