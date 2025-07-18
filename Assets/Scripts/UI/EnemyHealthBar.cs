using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    public Health health;
    public SpriteRenderer fill;

    private float fullFillWidth;

    private void Awake()
    {
        health = GetComponentInParent<Health>();
        fullFillWidth = transform.localScale.x;

        if (fill.gameObject.activeSelf)
        {
            foreach (Transform child in transform)
            {
                Debug.Log(child.name);
                child.gameObject.SetActive(false);
            }
        }
    }

    public void HandleHealthbar()
    {
        if (!fill.gameObject.activeSelf)
        {
            foreach (Transform child in transform)
            {
                Debug.Log(child.name);
                child.gameObject.SetActive(true);
            }
        }

        ScaleFill();
    }

    private void ScaleFill()
    {
        float normalizedHealth = Mathf.InverseLerp(0, health.maxHealth, health.currentHealth);
        normalizedHealth = Mathf.Clamp01(normalizedHealth);
        Vector3 newScale = fill.transform.localScale;
        newScale.x *= normalizedHealth;
        fill.transform.localScale = newScale;
    }


    private void OnEnable()
    {
        if (health == null)
        {
            Debug.Log(transform.parent.name);
            return;
        }
        health.OnDamaged += HandleHealthbar;
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleHealthbar;

    }

    private void OnDestroy()
    {
        health.OnDamaged -= HandleHealthbar;
    }
}
