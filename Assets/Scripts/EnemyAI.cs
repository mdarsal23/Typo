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

    void IdleState(float distance)
    {
        agent.isStopped = true;

        animator.SetFloat("Speed", 0f);

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
            animator.SetFloat("Speed", 0.3f); // idle with alert
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if(distance < 8f)
        {
            agent.speed = 4.5f;
            animator.SetFloat("Speed", 1f); // run
        }
        else
        {
            agent.speed = 2f;
            animator.SetFloat("Speed", 0.5f); // walk
        }

        if(distance < attackRange)
        {
            currentState = State.Attack;
        }
    }

    void AttackState(float distance)
    {
        agent.isStopped = true;

        transform.LookAt(player);

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