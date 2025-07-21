using UnityEngine;
using UnityEngine.Events;

public class StoryManager : MonoBehaviour
{
    public UnityEvent storyBeat;
    public static StoryManager Instance;
    public int enemiesToLevel;
    public int enemiesKilled;

    private void Awake()
    {
        Instance = this;
    }

    public void EnemyKilled()
    {
        Debug.Log("Yeah!");
        if (enemiesKilled >= enemiesToLevel)
        {
            return;
        }
        enemiesKilled++;
        if (enemiesKilled >= enemiesToLevel)
        {
            storyBeat?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }
}