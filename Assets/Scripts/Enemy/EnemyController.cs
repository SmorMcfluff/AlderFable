using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Wandering Settings")]
    public float wanderRange = 2;
    public float wanderMinCooldown = 0.5f;
    public float wanderMaxCooldown = 3f;

    [HideInInspector] public EnemySpawner owningSpawner;

    public Movement movement;
    public Attack attack;
    public Health health;

    [HideInInspector] public EnemyHealthBar healthBar;

    [HideInInspector] public List<IDamageable> targets = new List<IDamageable>();

    private bool canWander = true;
    private Coroutine wanderRoutine;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        attack = GetComponent<Attack>();
        health = GetComponent<Health>();
        healthBar = GetComponentInChildren<EnemyHealthBar>();
    }

    private void FixedUpdate()
    {
        if (!health.IsDead)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        if (targets.Count == 0)
        {
            Wander();
        }
        else
        {
            movement.Move(GetDirectionToTarget());
        }
        BoundsCheck();
    }

    private void Wander()
    {
        Platform currentPlatform = movement.currentPlatform;
        if (currentPlatform != null)
        {
            float platformLeft = currentPlatform.bounds.min.x;
            float platformRight = currentPlatform.bounds.max.x;

            float wanderLeft = transform.position.x - wanderRange;
            float wanderRight = transform.position.x + wanderRange;

            if (wanderLeft < platformLeft)
            {
                wanderLeft = platformLeft;
            }
            if (wanderRight > platformRight)
            {
                wanderRight = platformRight;
            }

            float targetX = Random.Range(wanderLeft, wanderRight);
            Vector2 targetPoint = new(targetX, transform.position.y);
            wanderRoutine = StartCoroutine(WanderCoroutine(targetPoint));
        }
    }

    private IEnumerator WanderCoroutine(Vector2 targetPoint)
    {
        if (!canWander) yield break;
        canWander = false;

        float speed = movement.movementSpeed;
        float distance = Vector2.Distance(transform.position, targetPoint);
        float timeToReachTarget = distance / speed;

        float elapsedTime = 0f;
        while (elapsedTime < timeToReachTarget)
        {
            elapsedTime += Time.deltaTime;
            Vector2 newPosition = Vector2.Lerp(transform.position, targetPoint, elapsedTime / timeToReachTarget);
            movement.Move(newPosition - (Vector2)transform.position);
            yield return null;
        }
        movement.Move(Vector2.zero);
        float cooldown = Random.Range(wanderMinCooldown, wanderMaxCooldown);
        yield return new WaitForSeconds(cooldown);
        canWander = true;
        wanderRoutine = null;
    }

    private Vector2 GetDirectionToTarget()
    {
        IDamageable target = GetNearestTarget();
        if (target == null) return Vector2.zero;
        
        Vector2 targetPosition = (target as Component).transform.position;
        float horizontalDistance = Mathf.Abs(targetPosition.x - transform.position.x);
        if (horizontalDistance < 0.1f) return Vector2.zero;

        float platformLeft = movement.currentPlatform.bounds.min.x;
        float platformRight = movement.currentPlatform.bounds.max.x;

        if (targetPosition.x < platformLeft)
        {
            targetPosition = new(platformLeft, targetPosition.y);
        }
        else if (targetPosition.x > platformRight)
        {
            targetPosition = new(platformRight, targetPosition.y);
        }
            Vector2 currentPosition = transform.position;

        return (targetPosition - currentPosition).normalized;
    }

    private IDamageable GetNearestTarget()
    {
        if (targets.Count == 0) return null;
        targets.RemoveAll(t => t == null || (t is Health h && h.IsDead));

        IDamageable closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var target in targets)
        {
            if (target is Health targetHealth && !targetHealth.IsDead)
            {
                float targetXPos = targetHealth.transform.position.x;
                float distance = Mathf.Abs(targetXPos - transform.position.x);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = target;
                }
            }
        }
        return closestTarget;
    }

    public void AddTarget(IDamageable newTarget)
    {
        if (newTarget != null && !targets.Contains(newTarget))
        {
            targets.Add(newTarget);
            if (wanderRoutine != null)
            {
                StopCoroutine(wanderRoutine);
                wanderRoutine = null;
            }
        }
    }

    private void BoundsCheck()
    {
        float platformLeft = movement.currentPlatform.bounds.min.x;
        float platformRight = movement.currentPlatform.bounds.max.x;
        Vector2 newPos = transform.position;
        if (transform.position.x < platformLeft)
        {
            newPos.x = platformLeft + 0.1f;
        }
        else if (transform.position.x > platformRight)
        {
            newPos.x = platformRight - 0.1f;
        }
        transform.position = newPos;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 boxSize = new Vector2(wanderRange * 2, 0.1f);
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
#endif
}
