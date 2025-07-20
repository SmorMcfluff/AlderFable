using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    public Image fadeImage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }


    public void StartFade(Color targetColor, float fadeDuration)
    {
        StartCoroutine(FadeCoroutine(targetColor, fadeDuration));
    }

    private IEnumerator FadeCoroutine(Color targetColor, float fadeDuration)
    {
        Color startColor = fadeImage.color;
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = targetColor;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeImage.color != Color.clear)
        {
            StartFade(Color.clear, 1f);
        }
    }
}
