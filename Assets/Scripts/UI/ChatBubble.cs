using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox;

    public Transform owner;
    public RectTransform rectTransform;
    public string userName;
    public float yOffset = 0.75f;

    [Header("Chat Bubble References")]
    public Image chatBubble;
    public Image bubbleHook;
    public Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (!chatBubble.gameObject.activeSelf) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(owner.position + Vector3.up * yOffset);
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void SendChat(string message)
    {
        chatBubble.gameObject.SetActive(true);
        bubbleHook.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ShowMessage(message));
    }

    private IEnumerator ShowMessage(string message)
    {
        textBox.text = $"{userName}: {message}";
        yield return new WaitForSeconds(5f);
        bubbleHook.gameObject.SetActive(false);
        chatBubble.gameObject.SetActive(false);
    }
}
