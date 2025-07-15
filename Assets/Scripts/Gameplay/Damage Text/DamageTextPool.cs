using System.Collections.Generic;
using UnityEngine;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance;
    public DamageText damageTextPrefab;
    public int poolSize = 10;

    private Queue<DamageText> pool = new Queue<DamageText>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            DamageText dt = Instantiate(damageTextPrefab, transform);
            dt.gameObject.SetActive(false);
            pool.Enqueue(dt);
        }
    }

    public DamageText Get()
    {
        DamageText dt = pool.Count > 0 ? pool.Dequeue() : Instantiate(damageTextPrefab, transform);
        dt.gameObject.SetActive(true);
        return dt;
    }

    public void Return(DamageText dt)
    {
        dt.gameObject.SetActive(false);
        pool.Enqueue(dt);
    }
}