using UnityEngine;

public class Portal : MonoBehaviour
{
    [HideInInspector] public Map parentMap;
    public Map destinationMap;
    public bool canBeEntered = true;

    public void EnterPortal()
    {
        Debug.Log("Entering Portal!");
        FadeManager.Instance.StartFade(Color.black, 1);
        Invoke(nameof(ExitPortal), 1.5f);
    }

    public void ExitPortal()
    {
        Map spawnedMap = Instantiate(destinationMap);
        Vector3 spawnPosInNewMap = spawnedMap.GetSpawnPoint(this, parentMap);
        spawnedMap.LoadMap(spawnPosInNewMap);
        PlayerInput player = FindFirstObjectByType<PlayerInput>();
        player.transform.position = spawnPosInNewMap;
        parentMap.DestroyMap();
        FadeManager.Instance.StartFade(Color.clear, 1);
        player.movement.isStunned = false;
    }
}
