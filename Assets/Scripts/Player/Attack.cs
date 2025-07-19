using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public Weapon equippedWeapon;
    LayerMask damageLayer;
    private PlayerAnimator anim;

    private bool readyToAttack = true;
    public bool ReadyToAttack => readyToAttack;
    public bool touchAttack = false;

    private EnemyController enemyController;

    private void Awake()
    {
        damageLayer = LayerMask.GetMask("Damageable");

        enemyController = GetComponent<EnemyController>();
        anim = GetComponent<PlayerAnimator>();
    }

    private void FixedUpdate()
    {
        if (touchAttack)
        {
            StartCoroutine(ExecuteAttack(Vector2.zero));
        }
    }

    public void TriggerAttack(FacingDirection direction)
    {
        if (!readyToAttack) return;
        var dir = SetDirection(direction);
        StartCoroutine(ExecuteAttack(dir));
    }

    private IEnumerator ExecuteAttack(Vector2 direction)
    {
        if (!touchAttack)
        {
            if (anim != null)
            {
                anim.SetSprite(anim.attackSprites[0]);
            }

            readyToAttack = false;
            yield return new WaitForSeconds(equippedWeapon.AttackWindup);
        }

        float hitboxHeight = transform.localScale.y * equippedWeapon.attackHeightFactor;
        Vector2 hitBoxSize = new(equippedWeapon.attackRange, hitboxHeight);
        Vector2 hitBoxOffset = direction * (equippedWeapon.attackRange / 2f);
        Vector2 boxOrigin = (Vector2)transform.position + hitBoxOffset;

        if (anim != null)
        {
            anim.SetSprite(anim.attackSprites[1]);
        }

        DrawHitBox(hitBoxSize, hitBoxOffset);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxOrigin, hitBoxSize, 0, damageLayer);

        foreach (var enemy in GetHitEnemies(hits))
        {
            enemy.TakeDamage(this, Random.Range(equippedWeapon.minDamage, equippedWeapon.maxDamage + 1));
            if (enemyController != null)
            {
                enemyController.AddTarget(enemy);
            }
        }

        if (!touchAttack)
        {
            StartCoroutine(AttackCooldown());
        }
    }

    private Vector2 SetDirection(FacingDirection direction)
    {
        return direction switch
        {
            FacingDirection.Left => Vector2.left,
            FacingDirection.Right => Vector2.right,
            _ => Vector2.right
        };
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(equippedWeapon.AttackDelay);
        readyToAttack = true;
    }

    private string GetTeamToAttack()
    {
        return tag switch
        {
            "Player" => "Enemy",
            "Enemy" => "Player",
            _ => "Enemy"
        };
    }

    private List<IDamageable> GetHitEnemies(Collider2D[] hits)
    {
        List<IDamageable> hitTargets = new();
        List<(IDamageable damageable, Vector3 position)> candidates = new();
        HashSet<GameObject> seenObjects = new();

        string opponentTeam = GetTeamToAttack();
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable damageable) && hit.CompareTag(opponentTeam))
            {
                GameObject obj = (damageable as Component).gameObject;
                if (seenObjects.Add(obj))
                {
                    candidates.Add((damageable, hit.transform.position));
                }
            }
        }

        var sorted = candidates
            .OrderBy(c => (c.position - transform.position).sqrMagnitude)
            .Take(equippedWeapon.maxEnemiesHit)
            .ToList();

        foreach (var (damageable, _) in sorted)
        {
            hitTargets.Add(damageable);
        }

        return hitTargets;
    }

    private void DrawHitBox(Vector2 hitBoxSize, Vector2 hitBoxOffset)
    {
        Vector2 center = (Vector2)transform.position;
        Vector2 halfSize = hitBoxSize / 2f;

        Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y) + hitBoxOffset;
        Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y) + hitBoxOffset;
        Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y) + hitBoxOffset;
        Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y) + hitBoxOffset;

        Debug.DrawLine(topLeft, topRight, Color.red, 0.2f);
        Debug.DrawLine(topRight, bottomRight, Color.red, 0.2f);
        Debug.DrawLine(bottomRight, bottomLeft, Color.red, 0.2f);
        Debug.DrawLine(bottomLeft, topLeft, Color.red, 0.2f);
    }
}
