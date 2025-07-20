using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    TextMeshPro tutorialText;
    private void Awake()
    {
        tutorialText = GetComponent<TextMeshPro>();
        tutorialText.renderer.sortingLayerName = "Foreground";
        tutorialText.text = "Welcome to AlderFable!\r\nUse your arrow keys to move";
    }

    public void SetText(string text)
    {
        tutorialText.text = text;
    }
}
