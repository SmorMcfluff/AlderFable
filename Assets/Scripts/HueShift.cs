using UnityEngine;
using UnityEngine.UI;

public class HueShifter : MonoBehaviour
{
    public float Speed = 1;
    private Image image;
    public AudioClip levelUpSound;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    void Start()
    {
        image.color = Color.HSVToRGB(.34f, .84f, .67f);
    }

    public void LevelUp()
    {
        image.enabled = true;
        GameManager.Instance.PlayLevelUp();
        Invoke(nameof(Hide), 1f);
    }

    private void Hide()
    {
        image.enabled = false;

    }

    void Update()
    {
        float h, s, v;
        Color.RGBToHSV(image.color, out h, out s, out v);

        image.color = Color.HSVToRGB(h + Time.deltaTime * .5f, s, v);
    }
}