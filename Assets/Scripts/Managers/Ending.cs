using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ending : MonoBehaviour
{
    private float pitchTime;
    public float pitchFrequency = 1f;
    public bool isSinewavePitchActive = false;

    public string[] messages;
    public int messageIndex;
    public Chatting chatter;
    public Button yesButton;
    public Button noButton;
    public Button whatButton;
    public AudioClip clip;

    private void Awake()
    {
        if (!isSinewavePitchActive)
        {
            HideAllButtons();
        }
    }

    private void Update()
    {
        if (isSinewavePitchActive)
        {
            pitchTime += Time.deltaTime * pitchFrequency;

            float sine = Mathf.Sin(pitchTime);
            float pitch = Mathf.Lerp(0.5f, 2f, (sine + 1f) / 2f);
            Debug.Log(pitch);
            GameManager.Instance.source.pitch = pitch;
        }
    }

    private void HideAllButtons()
    {
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        whatButton.gameObject.SetActive(false);
    }

    public void InvokeSendMessage()
    {
        Invoke(nameof(SendMessage), 1f);
        InvokeShowButtons();
    }

    public void InvokeShowButtons()
    {
        Invoke(nameof(HandleMessageButtons), 5f);
    }

    public void SendMessage()
    {
        chatter.SendChat(messages[messageIndex]); //<--This line
    }

    public void HandleMessageButtons()
    {
        //  Yes/No questions
        if (messageIndex == 0 || messageIndex == 1 || messageIndex == 4)
        {
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);
            whatButton.gameObject.SetActive(false);
        }
        else //Wait what
        {
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
            whatButton.gameObject.SetActive(true);
        }
    }

    public void HandleYesNo(bool yes)
    {
        HideAllButtons();
        if (messageIndex == 0)
        {
            if (yes)
            {
                messageIndex = 1;
                InvokeSendMessage();
            }
            else
            {
                messageIndex = 4;
                InvokeSendMessage();
            }
        }
        else if (messageIndex == 1)
        {
            if (yes)
            {
                messageIndex = 2;
                InvokeSendMessage();
            }
            else
            {
                messageIndex = 3;
                InvokeSendMessage();
            }
        }
        else if (messageIndex == 4)
        {
            if (yes)
            {
                messageIndex = 5;
                InvokeSendMessage();
            }
            else
            {
                messageIndex = 6;
                InvokeSendMessage();
            }
        }
    }

    public void WaitWhat()
    {
        GameManager.Instance.source.clip = clip;
        GameManager.Instance.source.Play();
        isSinewavePitchActive = true;
        SceneManager.LoadScene("Ending");
    }
}
