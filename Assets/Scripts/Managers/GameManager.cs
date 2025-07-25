using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public AudioSource source;
    public AudioClip levelUpSound;

    public string playerUsername;
    public string[] friendNicknames =
    {
        "JoeYabuki",
        "Saltlick",
        "Pooploser",
        "Wonderchili",
        "Cravedia",
        "Jazz",
        "Punch",
        "Steambath",
        "Wangblade",
        "Mango",
        "Rainbosupper",
        "Thrillho"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlayLevelUp()
    {
        source.PlayOneShot(levelUpSound);
    }
}
