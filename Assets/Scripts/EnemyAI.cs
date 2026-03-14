using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    public State currentState;
    Animator animator;
    public Transform player;
    bool canReach = false;

    public int damage = 10;

    public float detectionRange = 12f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private float lastAttackTime;

    private NavMeshAgent agent;

    // --- Animation rotation offsets (edit in Inspector) ---
    public float idleYawOffset = 0f;
    public float walkYawOffset = 0f;
    public float runYawOffset = 0f;
    public float attackYawOffset = 0f;

    // Helper function
    bool HasPathToPlayer()
    {
        NavMeshPath path = new NavMeshPath();

        if(agent.CalculatePath(player.position, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }

        return false;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = State.Idle;
        animator = GetComponent<Animator>();
    }

    float pathCheckTimer;

    void Update()
    {
        pathCheckTimer += Time.deltaTime;

        if(pathCheckTimer >= 0.5f)
        {
            canReach = HasPathToPlayer();
            pathCheckTimer = 0f;
        }

        if(currentState == State.Dead)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch(currentState)
        {
            case State.Idle:
                IdleState(distance);
                break;

            case State.Chase:
                ChaseState(distance);
                break;

            case State.Attack:
                AttackState(distance);
                break;
        }
    }

    void RotateTowardsPlayer(float yawOffset)
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        if(dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            targetRot *= Quaternion.Euler(0, yawOffset, 0);
            transform.rotation = targetRot;
        }
    }

    void IdleState(float distance)
    {
        agent.isStopped = true;

        animator.SetFloat("Speed", 0f);

        RotateTowardsPlayer(idleYawOffset);

        if(distance < detectionRange)
        {
            currentState = State.Chase;
        }
    }

    void ChaseState(float distance)
    {
        if(!canReach)
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0.3f);
            RotateTowardsPlayer(idleYawOffset);
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if(distance < 8f)
        {
            agent.speed = 0f;
            animator.SetFloat("Speed", 1f);
            RotateTowardsPlayer(runYawOffset);
        }
        else
        {
            agent.speed = 0f;
            animator.SetFloat("Speed", 0.5f);
            RotateTowardsPlayer(walkYawOffset);
        }

        if(distance < attackRange)
        {
            currentState = State.Attack;
        }
    }

    void AttackState(float distance)
    {
        agent.isStopped = true;

        RotateTowardsPlayer(attackYawOffset);

        animator.SetFloat("Speed", 0f);

        if(Time.time > lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("Attack");
            Attack();
            lastAttackTime = Time.time;
        }

        if(distance > attackRange)
        {
            currentState = State.Chase;
        }
    }

    void Attack()
    {
        Debug.Log("Enemy attacks player");
        player.GetComponent<ThirdPersonShooterController>().TakeDamage(damage);
    }

    public void Die()
    {
        currentState = State.Dead;

        agent.isStopped = true;

        animator.SetBool("Dead", true);

        Destroy(gameObject, 3f);
    }
}