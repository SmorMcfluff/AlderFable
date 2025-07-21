using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class Health : MonoBehaviour, IDamageable
{
    private AudioSource src;
    public AudioClip damageSound;
    [Header("Health Settings")]
    public int maxHealth = 25;
    public float invincibilityTime = 0.75f;
    public float redFlashTime = 0.1f;
    public bool useInvincibility = false;
    [HideInInspector] public bool isInvincible = false;
    public bool canDie = true;

    public int currentHealth;

    public Action OnDamaged;
    public UnityEvent OnDeath;
    [HideInInspector] public Movement movement;

    public bool IsDead => currentHealth <= 0;

    private EnemyController enemyController;
    private SpriteRenderer sr;
    [HideInInspector] public Color defaultColor;


    private void Awake()
    {
        movement = GetComponentInChildren<Movement>();
        sr = GetComponent<SpriteRenderer>();
        src = GetComponent<AudioSource>();
        enemyController = GetComponent<EnemyController>();
        defaultColor = sr.color;

        currentHealth = maxHealth;
    }

    public void TakeDamage(Attack attacker, int damage)
    {
        if (isInvincible || IsDead) return;
        if (!canDie)
        {
            currentHealth -= 0;
        }
        else
        {
            currentHealth -= damage;
        }

        OnDamaged?.Invoke();
        SpawnDamageText(damage);
        src.PlayOneShot(damageSound, 0.5f);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (enemyController != null)
        {
            IDamageable damageable = attacker.GetComponent<IDamageable>();
            enemyController.AddTarget(damageable);
        }

        Knockback(attacker, damage);

        if (!useInvincibility)
        {
            StartCoroutine(FlashRed());
        }
        else
        {
            isInvincible = true;
            StartCoroutine(InvincibilityCountdown());
        }
    }

    private void SpawnDamageText(int damage)
    {
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        DamageText dt = DamageTextPool.Instance.Get();
        dt.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        dt.SetDamage(damage);
        dt.Activate();
    }

    private void Die()
    {
        if (CompareTag("Enemy"))
        {
            if (enemyController.owningSpawner != null)
            {
                sr.color = Color.clear;
                movement.enabled = false;
                enemyController.targets.Clear();
                enemyController.enabled = false;
                enemyController.attack.enabled = false;
                enabled = false;
                enemyController.owningSpawner.RespawnEnemy();
                enemyController.healthBar.gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.EnemyKilled();
        }
        OnDeath?.Invoke();
    }

    private void Knockback(Attack attacker, int damage)
    {
        float damageRatio = (float)damage / maxHealth;
        if (damageRatio < 0.1f) return;

        float attackDirection = attacker.transform.position.x < transform.position.x ? 1f : -1f;
        float knockbackForce = attacker.equippedWeapon.knockbackForce;
        movement.Knockback(attackDirection, knockbackForce * damageRatio);
    }

    public IEnumerator InvincibilityCountdown()
    {
        yield return FlashRed();
        sr.color = Color.Lerp(defaultColor, Color.black, 0.25f);
        yield return new WaitForSeconds(invincibilityTime - redFlashTime);
        isInvincible = false;
        sr.color = defaultColor;
    }

    public IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(redFlashTime);
        if (!useInvincibility)
        {
            sr.color = defaultColor;
        }
    }
}