using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject mapElements;
    public Portal leftPortal;
    public Portal rightPortal;

    void Start()
    {
        if (leftPortal != null)
        {
            leftPortal.parentMap = this;
        }
        if (rightPortal != null)
        {
            rightPortal.parentMap = this;
        }
    }

    public void DestroyMap()
    {
        EnemyController[] enemiesInThisMap = GetComponentsInChildren<EnemyController>();
        foreach (var enemy in enemiesInThisMap)
        {
            Destroy(enemy.gameObject);
        }

        Destroy(gameObject);
    }

    public void LoadMap(Vector3 spawnPos)
    {
        mapElements.SetActive(true);
        foreach (var spawner in GetComponentsInChildren<EnemySpawner>())
        {
            spawner.SpawnEnemy();
        }
    }

    public Vector3 GetSpawnPoint(Portal originPortal, Map lastMap)
    {
        if (originPortal == lastMap.leftPortal)
        {
            return rightPortal.transform.position;
        }
        else
        {
            return leftPortal.transform.position;
        }
    }
}
