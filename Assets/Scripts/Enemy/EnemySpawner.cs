using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private static float respawnTime = 2f;
    private static float fadeDuration = 1f;

    public EnemyController enemyPrefab;
    private EnemyController spawnedEnemy;
    private SpriteRenderer enemySprite;

    private void Awake()
    {
        SpawnEnemy();
    }

    private void OnEnable()
    {
        SpawnEnemy();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        if (spawnedEnemy == null)
        {
            spawnedEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
            spawnedEnemy.name += "_Spawned";  // Helpful for debugging
            enemySprite = spawnedEnemy.GetComponent<SpriteRenderer>();
            spawnedEnemy.owningSpawner = this;
            spawnedEnemy.movement.currentPlatform = GetPlatform();
        }
    }

    public void RespawnEnemy()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);

        Color enemyColor = spawnedEnemy.health.defaultColor;
        spawnedEnemy.transform.position = transform.position;

        float alpha;
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            alpha = Mathf.Lerp(0, 1, t);
            enemyColor.a = alpha;
            enemySprite.color = enemyColor;

            elapsed += Time.deltaTime;
            yield return null;
        }
        enemySprite.color = enemyColor;
        spawnedEnemy.health.currentHealth = spawnedEnemy.health.maxHealth;
        spawnedEnemy.movement.enabled = true;
        spawnedEnemy.attack.enabled = true;

        spawnedEnemy.enabled = true;
        spawnedEnemy.healthBar.gameObject.SetActive(true);
        spawnedEnemy.healthBar.HandleHealthbar();
        spawnedEnemy.healthBar.HideChildren();
    }

    private Platform GetPlatform()
    {
        LayerMask mask = LayerMask.GetMask("Ground");
        return Physics2D.Raycast(transform.position, Vector2.down, 3, mask).collider.GetComponent<Platform>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.27f);

    }
}
