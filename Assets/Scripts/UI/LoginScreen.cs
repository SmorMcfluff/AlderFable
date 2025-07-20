using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{
    public TMP_InputField usernameField;
    public TextMeshProUGUI warningText;
    public Button loginButton;

    private void Awake()
    {
        usernameField.text = string.Empty;
        warningText.text = string.Empty;
    }

    public void AttemptLogin()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            warningText.text = "Enter a username!";
        }
        else if (usernameField.text.Length > 12)
        {
            warningText.text = "Username has to be less than 12 characters!";
        }
        else
        {
            Login();
        }
    }

    private void Login()
    {
        Debug.Log(usernameField.text);
        loginButton.enabled = false;
        FadeManager.Instance.StartFade(Color.black, 1f);
        GameManager.Instance.username = usernameField.text;
        Invoke(nameof(ChangeScene), 1);
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
