using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public MoneyManager moneyManager;
    public float meleeRange = 2f;
    public float moveSpeed = 5f;
    public float attackCooldown = 2f;
    public float tauntCooldown = 5f;
    public float tauntChance = 0.2f;
    public GameObject meleeHitbox;
    public List<AudioClip> tauntAudioClips;
    public float tauntVolume = 1f;
    public float wallDamage = 10f;
    public float ringDamage = 10f;


    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Transform target;
    private float attackTimer;
    private float tauntTimer;
    private bool isAttacking;
    private float originalSpeed;
    private EnemySpawner spawner;
    private bool isAttracted;
    private Vector3 attractionPoint;
    private float attractionTimer;
    private bool isDead;
    private bool isDrinking;
    private WallSection wallTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        spawner = FindObjectOfType<EnemySpawner>();
        originalSpeed = moveSpeed;
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        target = GameObject.FindGameObjectWithTag("Ring").transform;
        attackTimer = 0f;
        tauntTimer = 0f;
        isAttracted = false;
        attractionTimer = 0f;
        isDead = false;
        isDrinking = false;

        animator.SetBool("IsRunning", true);
        animator.SetBool("IsInMeleeRange", false);
    }

    void Update()
    {
        if (isAttacking || isDead || isDrinking) return;

        attackTimer -= Time.deltaTime;
        tauntTimer -= Time.deltaTime;

        if (isAttracted)
        {
            attractionTimer -= Time.deltaTime;
            if (attractionTimer <= 0f)
            {
                isAttracted = false;
            }

            float distanceToAttraction = Vector3.Distance(transform.position, attractionPoint);
            if (distanceToAttraction <= 0.5f)
            {
                Debug.Log($"{gameObject.name} reached attraction point, triggering Drink animation");
                StartCoroutine(PlayDrinkAnimation());
                return;
            }
        }

        Vector3 currentTargetPos = isAttracted ? attractionPoint : target.position;
        bool hasPath = HasPathTo(currentTargetPos);

        if (!hasPath && wallTarget == null)
        {
            wallTarget = FindNearestWallSection();
            if (wallTarget != null)
            {
                Debug.Log($"{gameObject.name} cannot reach target, targeting wall section {wallTarget.name}");
            }
        }

        Transform attackTarget = wallTarget != null ? wallTarget.transform : null;
        Vector3 finalTargetPos = wallTarget != null ? wallTarget.transform.position : currentTargetPos;
        float distanceToTarget = Vector3.Distance(transform.position, finalTargetPos);
        bool inMeleeRange = distanceToTarget <= meleeRange;

        animator.SetBool("IsInMeleeRange", inMeleeRange);

        if (inMeleeRange)
        {
            if (agent) agent.SetDestination(transform.position);
            animator.SetBool("IsRunning", false);

            if (attackTimer <= 0f)
            {
                Debug.Log($"{gameObject.name} initiating MeleeAttack");
                StartCoroutine(PlayMeleeAttack(attackTarget));
                attackTimer = attackCooldown;
                if (Random.value < tauntChance && tauntTimer <= 0f)
                {
                    StartCoroutine(PlayTaunt());
                    tauntTimer = tauntCooldown;
                }
            }
        }
        else
        {
            animator.SetBool("IsRunning", true);
            Vector3 dir = (finalTargetPos - transform.position).normalized;
            dir.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

            if (agent)
            {
                agent.SetDestination(finalTargetPos);
                agent.speed = moveSpeed;
            }
            else
            {
                transform.position += dir * moveSpeed * Time.deltaTime;
            }
        }
    }

    private bool HasPathTo(Vector3 targetPos)
    {
        if (agent == null) return false;

        NavMeshPath path = new NavMeshPath();
        bool pathFound = agent.CalculatePath(targetPos, path);
        return pathFound && path.status == NavMeshPathStatus.PathComplete;
    }

    private WallSection FindNearestWallSection()
    {
        WallSection[] wallSections = FindObjectsOfType<WallSection>();
        WallSection nearest = null;
        float minDistance = float.MaxValue;

        foreach (WallSection section in wallSections)
        {
            float distance = Vector3.Distance(transform.position, section.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = section;
            }
        }

        return nearest;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining Health: {health}");

        if (health <= 0f)
        {
            StartCoroutine(Die());
        }
        else
        {
            StartCoroutine(PlayDamageAnimation());
        }
    }

    IEnumerator PlayDamageAnimation()
    {
        if (isDead) yield break;
        isAttacking = true;
        if (agent) agent.SetDestination(transform.position);
        animator.SetBool("IsRunning", false);
        animator.SetTrigger("TakeDamage");

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false;
    }

    IEnumerator Die()
    {
        if (isDead) yield break;
        isDead = true;
        isAttacking = true;
        isAttracted = false;
        isDrinking = false;
        animator.SetTrigger("Die");
        if (agent) agent.SetDestination(transform.position);

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (moneyManager != null) moneyManager.AddMoney(10);
        if (spawner != null) spawner.EnemyDied();
        Destroy(gameObject);
    }

    IEnumerator PlayMeleeAttack(Transform attackTarget)
    {
        if (isDead) yield break;
        isAttacking = true;
        animator.SetTrigger("MeleeAttack");
        if (meleeHitbox) meleeHitbox.SetActive(true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        if (meleeHitbox) meleeHitbox.SetActive(false);

        if (attackTarget != null && attackTarget.GetComponent<WallSection>() != null)
        {
            WallSection section = attackTarget.GetComponent<WallSection>();
            section.TakeDamage(wallDamage);
        }

        if (attackTarget != null && attackTarget.GetComponent<RingLogic>() != null)
        {
            RingLogic section = attackTarget.GetComponent<RingLogic>();
            section.TakeDamage(ringDamage);
        }

        isAttacking = false;
    }

    IEnumerator PlayTaunt()
    {
        if (isDead) yield break;
        isAttacking = true;
        animator.SetTrigger("Taunt");
        if (tauntAudioClips != null && tauntAudioClips.Count > 0)
        {
            AudioClip selectedTaunt = tauntAudioClips[Random.Range(0, tauntAudioClips.Count)];
            audioSource.PlayOneShot(selectedTaunt, tauntVolume);
        }
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false;
    }

    IEnumerator PlayDrinkAnimation()
    {
        if (isDead) yield break;
        isDrinking = true;
        if (agent) agent.SetDestination(transform.position);
        animator.SetBool("IsRunning", false);
        animator.SetTrigger("Drink");
        Debug.Log($"{gameObject.name} set Drink trigger");

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isDrinking = false;
        isAttracted = false;
        Debug.Log($"{gameObject.name} finished Drink animation");
    }

    public void ApplySlow(float slowAmount, float duration)
    {
        if (isDead) return;
        StopCoroutine("RemoveSlow");
        moveSpeed = Mathf.Max(0, moveSpeed - slowAmount);
        if (agent) agent.speed = moveSpeed;
        StartCoroutine(RemoveSlow(slowAmount, duration));
    }

    IEnumerator RemoveSlow(float slowAmount, float duration)
    {
        yield return new WaitForSeconds(duration);
        moveSpeed += slowAmount;
        moveSpeed = Mathf.Min(moveSpeed, originalSpeed);
        if (agent) agent.speed = moveSpeed;
    }

    public void AttractTo(Vector3 position, float duration)
    {
        if (isDead) return;
        isAttracted = true;
        attractionPoint = position;
        attractionTimer = duration;
        wallTarget = null;
        Debug.Log($"{gameObject.name} is attracted to {position} for {duration} seconds");
    }

    void OnAttackHitboxEvent(bool enable)
    {
        if (meleeHitbox) meleeHitbox.SetActive(enable);
    }
}