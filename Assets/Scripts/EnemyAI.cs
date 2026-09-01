using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { LocatingTarget, Moving, Attacking, Dead }
    
    [Header("State")]
    public EnemyState currentState = EnemyState.LocatingTarget;

    [Header("Attack Settings")]
    [Tooltip("How close the enemy needs to be to hit the building.")]
    public float attackRange = 2f;
    [Tooltip("Time between consecutive attacks (not after one has finished) in seconds.")]
    public float attackCooldown = 1.5f;

    [Header("Animation Timing (No Events Needed)")]
    [Tooltip("The total duration of the attack animation clip in seconds. The enemy will stand still for this long.")]
    public float totalAttackAnimationTime = 1.2f;
    [Tooltip("How many seconds into the animation until the hit actually connects and deals damage.")]
    public float timeTillHit = 0.5f;

    [Header("Animation Parameters")]
    [Tooltip("The name of the float parameter in your Animator controlling movement speed.")]
    public string speedParamName = "Speed";
    [Tooltip("The name of the trigger parameter in your Animator for attacking.")]
    public string attackTriggerName = "Attack";

    private NavMeshAgent agent;
    private Animator animator;
    private GameObject currentTarget;
    
    private bool isAttacking = false;
    private float attackStartTime = 0f;
    private bool damageDealtThisCycle = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        agent.stoppingDistance = attackRange - 0.2f;
    } 

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        UpdateAnimatorSpeed();

        switch (currentState)
        {
            case EnemyState.LocatingTarget:
                FindNextTarget();
                break;

            case EnemyState.Moving:
                MoveToTarget();
                break;

            case EnemyState.Attacking:
                AttackTarget();
                break;
        }
    }

    private void FindNextTarget()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building"); 
        GameObject core = GameObject.FindWithTag("CoreBeacon");

        GameObject closestTarget = GetClosestTargetWithCore(buildings, core);

        if (closestTarget != null)
        {
            currentTarget = closestTarget;
            SetDestinationToTargetEdge();
            currentState = EnemyState.Moving;
        }
    }

    //<>
    private void SetDestinationToTargetEdge()
    {
        if (currentTarget == null) return;

        Vector3 destination = currentTarget.transform.position; // Fallback default destination coordinate set to the exact center pivot of the target object

        if (currentTarget.TryGetComponent<Collider>(out Collider targetCollider)) // Checks if the target has a physical Collider component attached, caching it as targetCollider
        {
            destination = targetCollider.ClosestPointOnBounds(transform.position); // Uses the collider geometry to find the mathematical nearest point to our enemy's body
        }

        agent.SetDestination(destination); // Commands the underlying NavMesh routing engine to compute and execute a path to that point
    }

    private void MoveToTarget()
    {
        if (currentTarget == null)
        {
            currentState = EnemyState.LocatingTarget;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = EnemyState.Attacking;
        }
    }

    private void AttackTarget()
    {
        if (!isAttacking)
        {
            if (currentTarget == null)
            {
                ResetAttackState();
                currentState = EnemyState.LocatingTarget;
                return;
            }

            isAttacking = true;
            damageDealtThisCycle = false;
            attackStartTime = Time.time;

            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            animator.SetTrigger(attackTriggerName);
        }

        // Facing Target in case model has an angle towards it
        if (currentTarget != null)
        {
            FaceTarget(currentTarget.transform.position);
        }
        
        // Dealing damage
        if (isAttacking && !damageDealtThisCycle && Time.time - attackStartTime >= timeTillHit)
        {
            if (currentTarget != null)
            {
                InteractWithBuilding(currentTarget);
            }

            damageDealtThisCycle = true;
        }


        // Waiting for animation to finish
        float totalRequiredWaitTime = Mathf.Max(totalAttackAnimationTime, attackCooldown);

        if (isAttacking && Time.time - attackStartTime >= totalRequiredWaitTime)
        {
            isAttacking = false;
            agent.isStopped = false;

            if (currentTarget == null)
            {
                currentState = EnemyState.LocatingTarget;
            }
        }
    }

    private void ResetAttackState()
    {
        isAttacking = false;
        damageDealtThisCycle = false;
        agent.isStopped = false;
    }

    //* add input variables for Damage amount and adjust tower and core scripts accordingly
    private void InteractWithBuilding(GameObject target)
    {
        if (target.TryGetComponent<DefensiveTower>(out DefensiveTower tower))
        {
            tower.TakeDamage();
        }
        else if (target.TryGetComponent<CoreBeacon>(out CoreBeacon core))
        {
            core.TakeDamage();
        }
    }

    // Lets Animator decide which animation should be played (e.g. Speed >= 0.1 --> Walking)
    private void UpdateAnimatorSpeed()
    {
        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat(speedParamName, currentSpeed);
    }

    //<>
    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized; 
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    private GameObject GetClosestTargetWithCore(GameObject[] buildings, GameObject core)
    {
        GameObject bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject tower in buildings)
        {
            if (tower == null) continue;
            float dSqr = (tower.transform.position - currentPosition).sqrMagnitude;
            if (dSqr < closestDistanceSqr)
            {
                closestDistanceSqr = dSqr;
                bestTarget = tower;
            }
        }

        if (core != null)
        {
            float dSqrToCore = (core.transform.position - currentPosition).sqrMagnitude;
            if (dSqrToCore < closestDistanceSqr)
            {
                bestTarget = core;
            }
        }

        return bestTarget;
    }
}