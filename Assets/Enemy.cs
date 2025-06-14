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

    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Transform target;
    private float attackTimer;
    private float tauntTimer;
    private bool isAttacking;
    private float originalSpeed;
    private EnemySpawner spawner;
    private bool isAttracted; // Tracks if enemy is attracted to bait
    private Vector3 attractionPoint; // Position of the bait
    private float attractionTimer; // Time remaining for attraction

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

        animator.SetBool("IsRunning", true);
        animator.SetBool("IsInMeleeRange", false);
    }

    void Update()
    {
        if (isAttacking || health <= 0f) return; // Skip if attacking or dead

        // Update timers
        attackTimer -= Time.deltaTime;
        tauntTimer -= Time.deltaTime;

        // Handle attraction timer
        if (isAttracted)
        {
            attractionTimer -= Time.deltaTime;
            if (attractionTimer <= 0f)
            {
                isAttracted = false; // End attraction
            }
        }

        // Determine current target position
        Vector3 currentTargetPos = isAttracted ? attractionPoint : target.position;
        float distanceToTarget = Vector3.Distance(transform.position, currentTargetPos);
        bool inMeleeRange = distanceToTarget <= meleeRange;

        // Update animator parameters
        animator.SetBool("IsInMeleeRange", inMeleeRange);

        if (inMeleeRange)
        {
            // Stop moving
            if (agent) agent.SetDestination(transform.position);
            animator.SetBool("IsRunning", false);

            // Attack if cooldown is ready
            if (attackTimer <= 0f)
            {
                StartCoroutine(PlayMeleeAttack());
                attackTimer = attackCooldown;
                // Random chance to taunt after attack
                if (Random.value < tauntChance && tauntTimer <= 0f)
                {
                    StartCoroutine(PlayTaunt());
                    tauntTimer = tauntCooldown;
                }
            }
        }
        else
        {
            // Move toward current target
            animator.SetBool("IsRunning", true);
            Vector3 dir = (currentTargetPos - transform.position).normalized;
            dir.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

            if (agent)
            {
                agent.SetDestination(currentTargetPos);
                agent.speed = moveSpeed;
            }
            else
            {
                transform.position += dir * moveSpeed * Time.deltaTime;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining Health: {health}");

        if (health > 0)
        {
            StartCoroutine(PlayDamageAnimation());
        }
        else
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator PlayDamageAnimation()
    {
        isAttacking = true;
        if (agent) agent.SetDestination(transform.position);
        animator.SetBool("IsRunning", false);
        animator.SetTrigger("TakeDamage");

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false;
    }

    IEnumerator Die()
    {
        isAttacking = true;
        isAttracted = false; // Stop attraction on death
        animator.SetTrigger("Die");
        if (agent) agent.SetDestination(transform.position);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        if (moneyManager != null) moneyManager.AddMoney(10);
        if (spawner != null) spawner.EnemyDied();
        Destroy(gameObject);
    }

    IEnumerator PlayMeleeAttack()
    {
        isAttacking = true;
        animator.SetTrigger("MeleeAttack");
        if (meleeHitbox) meleeHitbox.SetActive(true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        if (meleeHitbox) meleeHitbox.SetActive(false);
        isAttacking = false;
    }

    IEnumerator PlayTaunt()
    {
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

    public void ApplySlow(float slowAmount, float duration)
    {
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
        if (health <= 0f || isAttacking) return; // Ignore if dead or attacking

        isAttracted = true;
        attractionPoint = position;
        attractionTimer = duration;
        Debug.Log($"{gameObject.name} is attracted to {position} for {duration} seconds");
    }

    void OnAttackHitboxEvent(bool enable)
    {
        if (meleeHitbox) meleeHitbox.SetActive(enable);
    }
}