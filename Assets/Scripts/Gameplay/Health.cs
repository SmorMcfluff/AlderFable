using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 25;
    public float invincibilityTime = 0.75f;
    public float redFlashTime = 0.1f;
    public bool isInvincible = false;

    [SerializeField] private int currentHealth;

    private SpriteRenderer sr;
    private Color defaultColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

        SpawnDamageText(damage);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        isInvincible = true;
        StartCoroutine(InvincibilityCountdown());
    }

    private void SpawnDamageText(int damage)
    {
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        DamageText dt = DamageTextPool.Instance.Get();
        dt.transform.position = spawnPos;
        dt.transform.rotation = Quaternion.identity;
        dt.SetDamage(damage);
        dt.Activate();
    }



    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
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
    }
}