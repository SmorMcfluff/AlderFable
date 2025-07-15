using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float riseSpeed = 1f;
    public float timeUntilFade = 0.75f;
    public float fadeDuration = 0.25f;

    private TextMeshPro text;
    private Color originalColor;
    private Vector3 startPosition;

    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
        originalColor = text.color;
    }

    private void OnEnable()
    {
        text.color = originalColor;
        startPosition = transform.position;
    }

    public void Activate()
    {
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float elapsedTime = 0f;

        while (elapsedTime < timeUntilFade)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
            Color c = text.color;
            c.a = alpha;
            text.color = c;

            fadeElapsed += Time.deltaTime;
            yield return null;
        }

        Color finalColor = text.color;
        finalColor.a = 0f;
        text.color = finalColor;

        DamageTextPool.Instance.Return(this);
    }

    public void SetDamage(int damage)
    {
        text.text = damage.ToString();
    }
}
