using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attack : MonoBehaviour
{
    LayerMask damageLayer;
    bool readyToAttack = true;

    public Weapon equippedWeapon;

    private void Awake()
    {
        damageLayer = LayerMask.GetMask("Damageable");
    }

    public void TriggerAttack(FacingDirection direction)
    {
        if (!readyToAttack) return;
        var dir = SetDirection(direction);
        StartCoroutine(ExecuteAttack(dir));
    }

    private IEnumerator ExecuteAttack(Vector2 direction)
    {
        readyToAttack = false;
        yield return new WaitForSeconds(equippedWeapon.AttackWindup);

        Vector2 hitBoxSize = new(equippedWeapon.attackRange, transform.localScale.y);
        Vector2 hitBoxOffset = direction * (equippedWeapon.attackRange / 2f);
        Vector2 boxOrigin = (Vector2)transform.position + hitBoxOffset;

        DrawHitBox(hitBoxSize, hitBoxOffset);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(boxOrigin, hitBoxSize, 0, hitBoxOffset, 0, damageLayer);

        foreach (var enemies in GetHitEnemies(hits))
        {
            enemies.TakeDamage(Random.Range(equippedWeapon.minDamage, equippedWeapon.maxDamage + 1));
        }

        StartCoroutine(AttackCooldown());
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

    private List<IDamageable> GetHitEnemies(RaycastHit2D[] hits)
    {
        List<IDamageable> hitTargets = new();
        List<(IDamageable damageable, Vector3 position)> candidates = new();

        string opponentTeam = GetTeamToAttack();
        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable) &&
                hit.collider.CompareTag(opponentTeam))
            {
                candidates.Add((damageable, hit.transform.position));
            }
        }

        var sorted = candidates
            .OrderBy(c => (c.position - transform.position).sqrMagnitude)
            .Take(equippedWeapon.maxEnemiesHit)
            .ToList();

        foreach (var (damageable, _) in sorted)
        {
            if (!hitTargets.Contains(damageable))
            {
                hitTargets.Add(damageable);
            }
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
