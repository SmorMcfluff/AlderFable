using System.Collections;
using UnityEngine;

public class Attack : MonoBehaviour
{
    LayerMask damageLayer = LayerMask.GetMask("Damageable");
    float attackDelay;
    bool readyToAttack = true;

    public Weapon weapon;

    public void TriggerAttack(Vector2 direction)
    {

    }

    public void ExecuteAttack(Vector2 direction)
    {
        if(!readyToAttack) return;
        Vector2 hitBoxSize = new(weapon.attackRange, transform.localScale.y);
        StartCoroutine(AttackCooldown());
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, hitBoxSize, 0, Vector2.zero, 0, damageLayer);
    }
    public void ExecuteAttack(Vector2 hitboxSize, int damage, Vector2 direction, float distance)
    {
        if (!readyToAttack) return;

        StartCoroutine(AttackCooldown());
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, hitboxSize, 0, direction, distance, damageLayer);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag(GetTeamToAttack()))
                {
                    hit.collider.GetComponent<IDamageable>().TakeDamage(damage);
                }
            }
        }
    }

    private IEnumerator AttackCooldown()
    {
        readyToAttack = false;
        yield return new WaitForSeconds(attackDelay);
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

    public void SetAttackSpeed(float weaponDelay)
    {
        attackDelay = weaponDelay;
    }
}
