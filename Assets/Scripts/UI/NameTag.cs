using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public Transform owner;
    public RectTransform rectTransform;
    public Canvas canvas;
    public float yOffset = 0.75f;

    private void Awake()
    {
        usernameText = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (owner.GetComponent<PlayerInput>() && GameManager.Instance != null)
        {
            usernameText.text = GameManager.Instance.playerUsername;
        }
        else if (owner.TryGetComponent(out NPCController npc))
        {
            usernameText.text = GameManager.Instance.friendNicknames[npc.friendIndex];
        }
    }

    private void LateUpdate()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(owner.position + Vector3.down * yOffset);
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }
}
