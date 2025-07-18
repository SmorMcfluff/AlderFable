using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    private Movement movement;
    private Attack attack;

    private const float retreatThresholdFactor = 1.3f;
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

            Vector2 myPos = transform.position;
            Vector2 enemyPos = enemy.transform.position;

            if (!IsOnSamePlatform(enemy))
            {
                NavigateToEnemyPlatform(enemy, transform.position);
                return;
            }
            else
            {
                float enemyDistance = GetEnemyDistance(currentTarget);
                FacingDirection enemyDirection = (enemyPos.x > myPos.x) ? FacingDirection.Right : FacingDirection.Left;
                if (movement.facingDirection != enemyDirection)
                {
                    movement.FlipCharacter();
                }

                MakeCombatDecision(enemyDistance, attackRange, enemyAttackRange, myPos, enemyPos, enemyDirection);
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
            List<Platform> platformPath = FindPlatformPath(movement.currentPlatform, enemy.movement.currentPlatform);
            if (platformPath == null || platformPath.Count < 2)
            {
                return float.MaxValue;
            }

            Platform nextPlatform = platformPath[1];

            Ladder ladder = movement.currentPlatform.ladders
                .FirstOrDefault(l => l.topPlatform == nextPlatform || l.bottomPlatform == nextPlatform);

            if (ladder == null)
            {
                return float.MaxValue;
            }

            float ladderXPos = ladder.transform.position.x;
            float ladderHeight = ladder.Height;

            float distanceToLadder = Mathf.Abs(ladderXPos - transformXPos);
            float distanceEnemyToLadder = Mathf.Abs(enemyXPos - ladderXPos);

            totalWalkDistance = distanceToLadder + ladderHeight + distanceEnemyToLadder;
        }
        return totalWalkDistance;
    }

    private bool IsOnSamePlatform(EnemyController enemy)
    {
        return enemy.movement.currentPlatform == movement.currentPlatform;
    }

    private void NavigateToEnemyPlatform(EnemyController enemy, Vector2 myPos)
    {
        Platform currentPlatform = movement.currentPlatform;
        Platform enemyPlatform = enemy.movement.currentPlatform;

        if (currentPlatform == null || enemyPlatform == null)
        {
            return;
        }
        if (currentPlatform == enemyPlatform)
        {
            return;
        }

        List<Platform> platformPath = FindPlatformPath(currentPlatform, enemyPlatform);
        if (platformPath == null || platformPath.Count < 2)
        {
            return;
        }

        Platform nextPlatform = platformPath[1];
        if (AttemptDownJump(nextPlatform, myPos.y))
        {
            movement.Jump(Vector2.down);
            return;
        }

        //Ladder Stuff==========================================================
        Ladder ladderToUse = currentPlatform.ladders
            .FirstOrDefault(l => l.topPlatform == nextPlatform || l.bottomPlatform == nextPlatform);

        if (ladderToUse == null)
        {
            return;
        }

        Vector2 ladderPos = ladderToUse.transform.position;
        Vector2 dirToLadder = new(ladderPos.x - myPos.x, 0f);

        if (Mathf.Abs(dirToLadder.x) > 0.1f)
        {
            movement.Move(new(Mathf.Sign(dirToLadder.x), 0f));
            return;
        }

        float dirToLadderTarget = Mathf.Sign(nextPlatform.Top - myPos.y);
        Debug.Log(dirToLadderTarget);
        movement.Move(new(0, dirToLadderTarget));
    }

    private bool AttemptDownJump(Platform nextPlatform, float myPosY)
    {
        if (movement.currentPlatform.isTwoWay)
        {
            return nextPlatform.transform.position.y < myPosY;
        }
        else return false;
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

    private List<Platform> FindPlatformPath(Platform start, Platform goal, HashSet<Platform> visited = null)
    {
        if (start == goal)
        {
            return new List<Platform> { start };
        }
        visited ??= new HashSet<Platform>(); //If null, assign
        if (!visited.Add(start))
        {
            return null;
        }

        foreach (Ladder ladder in start.ladders)
        {
            Platform connectedPlatforms = (start == ladder.topPlatform) ? ladder.bottomPlatform : ladder.topPlatform;
            var path = FindPlatformPath(connectedPlatforms, goal, visited);
            if (path != null)
            {
                path.Insert(0, start);
                return path;
            }
        }
        return null;
    }

    private void MakeCombatDecision(float enemyDistance, float attackRange, float enemyAttackRange, Vector2 myPos, Vector2 enemyPos, FacingDirection enemyDirection)
    {
        if (IsInRange(attackRange, enemyAttackRange, enemyDistance))
        {
            movement.Move(Vector2.zero);
            attack.TriggerAttack(enemyDirection);
            return;
        }

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