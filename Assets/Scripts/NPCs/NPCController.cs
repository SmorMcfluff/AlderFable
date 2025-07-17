using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    private Movement movement;
    private Attack attack;

    private const float retreatThresholdFactor = 1.2f;
    private const float approachThresholdFactor = 0.9f;

    [SerializeField] private NPCStates currentState = NPCStates.Idle;
    [SerializeField] private Collider2D currentHuntingArea;

    private IDamageable currentTarget;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        attack = GetComponent<Attack>();
    }


    private void FixedUpdate()
    {
        ExecuteStateBehavior();
    }

    private void ExecuteStateBehavior()
    {
        switch (currentState)
        {
            case NPCStates.Idle:
                movement.Move(Vector2.zero);
                break;
            case NPCStates.Hunting:
                Hunt();
                break;
            case NPCStates.FollowPlayer:
                // Handle following player behavior
                break;
            case NPCStates.Wandering:
                // Handle wandering behavior
                break;
            case NPCStates.MovingToTarget:
                // Handle moving to target behavior
                break;
        }
    }

    private void Hunt()
    {
        if (currentTarget == null || (currentTarget as Health).IsDead)
        {
            currentTarget = GetNearestEnemy();
        }
        if (currentTarget != null && !(currentTarget as Health).IsDead)
        {
            EnemyController enemy = (currentTarget as Component).GetComponent<EnemyController>();
            float attackRange = attack.equippedWeapon.attackRange;
            float enemyAttackRange = enemy.attack.equippedWeapon.attackRange;

            Vector2 colliderSize = movement.GetColliderBounds().size;

            Vector2 myPos = transform.position;
            Vector2 enemyPos = enemy.transform.position;

            if (enemy.movement.currentPlatform != movement.currentPlatform)
            {
                if (!movement.IsOnLadder)
                {

                    Ladder nearbyLadder = GetLadderAtPosition(transform.position, colliderSize);
                    bool isNearLadder = (nearbyLadder != null);

                    if (isNearLadder)
                    {
                        Vector2 dirToLadder = nearbyLadder.transform.position.y > myPos.y ? Vector2.up : Vector2.down;
                        movement.Move(dirToLadder);
                    }
                    else
                    {
                        float ladderXPos = GetNearestLadder().transform.position.x;
                        float ladderDistance = ladderXPos - myPos.x;
                        Vector2 dirToLadder = new Vector2(ladderDistance, 0f).normalized;
                        movement.Move(dirToLadder);
                    }
                }
                else
                {
                    if (movement.currentPlatform != null)
                    {
                        movement.SetCurrentPlatform(null);
                    }
                    Vector2 dirToEnemy = enemyPos.y > myPos.y ? Vector2.up : Vector2.down;
                    movement.Move(dirToEnemy);
                }
            }
            else
            {
                float enemyDistance = GetEnemyDistance(currentTarget);
                FacingDirection enemyDirection = (enemyPos.x > myPos.x) ? FacingDirection.Right : FacingDirection.Left;
                if (movement.facingDirection != enemyDirection)
                {
                    movement.FlipCharacter();
                }

                if (IsInRange(attackRange, enemyAttackRange, enemyDistance))
                {
                    movement.Move(Vector2.zero);
                    attack.TriggerAttack(enemyDirection);
                }
                else
                {
                    Vector2 direction;

                    if (ShouldRetreat(enemyDistance, enemyAttackRange))
                    {
                        direction = myPos.x < enemyPos.x ? Vector2.left : Vector2.right;
                    }
                    else if (ShouldApproach(enemyDistance, attackRange))
                    {
                        direction = myPos.x < enemyPos.x ? Vector2.right : Vector2.left;
                    }
                    else
                    {
                        movement.Move(Vector2.zero);
                        attack.TriggerAttack(enemyDirection);
                        return;
                    }
                    movement.Move(direction);
                }
            }
        }
    }

    private IDamageable GetNearestEnemy()
    {
        Bounds currentBounds = currentHuntingArea.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(currentBounds.center, currentBounds.size, 0f, LayerMask.GetMask("Damageable"));

        if (hits.Length == 0) return null;

        List<EnemyController> enemies = new();
        HashSet<IDamageable> seenObjects = new();
        Dictionary<IDamageable, float> enemyDistances = new();

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable damageable) && hit.CompareTag("Enemy"))
            {
                EnemyController enemy = (damageable as Component).GetComponent<EnemyController>();
                if (seenObjects.Add(damageable))
                {
                    enemyDistances[damageable] = GetEnemyDistance(damageable);
                }
            }
        }

        return enemyDistances
             .OrderBy(kv => kv.Value)
             .Select(kv => kv.Key)
             .FirstOrDefault();
    }

    private float GetEnemyDistance(IDamageable damageable)
    {
        EnemyController enemy = (damageable as Component).GetComponent<EnemyController>();
        float transformXPos = transform.position.x;
        float enemyXPos = enemy.transform.position.x;
        float totalWalkDistance;

        if (enemy.movement.currentPlatform == movement.currentPlatform)
        {
            totalWalkDistance = Mathf.Abs(enemyXPos - transformXPos);
        }
        else
        {
            Ladder nearestLadder = GetNearestLadder();
            float ladderXPos = nearestLadder.transform.position.x;
            float ladderHeight = nearestLadder.Height;

            float distanceToLadder = Mathf.Abs(ladderXPos - transformXPos);
            float distanceEnemyToLadder = Mathf.Abs(enemyXPos - ladderXPos);

            totalWalkDistance = distanceToLadder + ladderHeight + distanceEnemyToLadder;
        }
        return totalWalkDistance;
    }

    private bool IsInRange(float attackRange, float enemyAttackRange, float distanceToEnemy)
    {
        float dangerZone = enemyAttackRange * 0.5f;
        float rangeCompensation = 0.8f;
        float compensatedRange = attackRange * rangeCompensation;

        return dangerZone < distanceToEnemy && distanceToEnemy < compensatedRange;
    }

    private Ladder GetLadderAtPosition(Vector2 position, Vector2 size)
    {
        Collider2D ladderCollider = Physics2D.OverlapBox(position, size, 0, LayerMask.GetMask("Ladder"));
        return ladderCollider != null ? ladderCollider.GetComponent<Ladder>() : null;
    }

    private Ladder GetNearestLadder()
    {
        var ladderAtPos = GetLadderAtPosition(transform.position, movement.GetColliderBounds().size);
        if (ladderAtPos != null) return ladderAtPos;

        Bounds currentBounds = currentHuntingArea.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(currentBounds.center, currentBounds.size, 0f, LayerMask.GetMask("Ladder"));
        if (hits.Length == 0) return null;

        List<Ladder> ladders = new List<Ladder>();

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out Ladder ladder))
            {
                ladders.Add(ladder);
            }
        }

        var sorted = ladders
            .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
            .ToList();
        return sorted.FirstOrDefault();
    }

    private bool ShouldRetreat(float distance, float enemyAttackRange) =>
        distance < enemyAttackRange * retreatThresholdFactor;

    private bool ShouldApproach(float distance, float attackRange) =>
        distance > attackRange * approachThresholdFactor;

    public void SetHuntingArea(Collider2D newArea)
    {
        currentHuntingArea = newArea;
    }

    private void OnDrawGizmosSelected()
    {
        if (currentHuntingArea != null)
        {
            var bounds = currentHuntingArea.bounds;
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}