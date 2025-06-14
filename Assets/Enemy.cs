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
    public List<AudioClip> tauntAudioClips; // List of taunt audio clips
    public float tauntVolume = 1f;

    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Transform target;
    private float attackTimer;
    private float tauntTimer;
    private bool isAttacking;
    private float originalSpeed;
    private EnemySpawner spawner; // Reference to spawner

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        spawner = FindObjectOfType<EnemySpawner>(); // Find spawner
        originalSpeed = moveSpeed;
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        target = GameObject.FindGameObjectWithTag("Ring").transform;
        attackTimer = 0f;
        tauntTimer = 0f;

        animator.SetBool("IsRunning", true);
        animator.SetBool("IsInMeleeRange", false);
    }

    void Update()
    {
        if (isAttacking || health <= 0f) return; // Skip if attacking or dead

        // Update timers
        attackTimer -= Time.deltaTime;
        tauntTimer -= Time.deltaTime;

        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        bool inMeleeRange = distanceToTarget <= meleeRange;

        // Update animator parameters
        animator.SetBool("IsInMeleeRange", inMeleeRange);

        if (inMeleeRange)
        {
            // Stop moving
            if (agent) agent.SetDestination(transform.position);
            else transform.position = transform.position;

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
            // Move toward target
            animator.SetBool("IsRunning", true);
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

            if (agent)
            {
                agent.SetDestination(target.position);
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
        isAttacking = true; // Pause movement and other actions
        if (agent) agent.SetDestination(transform.position); // Stop NavMeshAgent
        animator.SetBool("IsRunning", false); // Stop running animation
        animator.SetTrigger("TakeDamage"); // Trigger damage animation

        // Wait for the damage animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false; // Resume normal behavior
    }

    IEnumerator Die()
    {
        isAttacking = true;
        animator.SetTrigger("Die");
        if (agent) agent.SetDestination(transform.position); // Stop movement
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        if (moneyManager != null) moneyManager.AddMoney(10);
        if (spawner != null) spawner.EnemyDied(); // Notify spawner
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
        StopCoroutine("RemoveSlow"); // Prevent stacking slow resets
        moveSpeed = Mathf.Max(0, moveSpeed - slowAmount);
        if (agent) agent.speed = moveSpeed; // Update NavMeshAgent speed
        StartCoroutine(RemoveSlow(slowAmount, duration));
    }

    IEnumerator RemoveSlow(float slowAmount, float duration)
    {
        yield return new WaitForSeconds(duration);
        moveSpeed += slowAmount;
        moveSpeed = Mathf.Min(moveSpeed, originalSpeed);
        if (agent) agent.speed = moveSpeed; // Restore NavMeshAgent speed
    }

    void OnAttackHitboxEvent(bool enable)
    {
        if (meleeHitbox) meleeHitbox.SetActive(enable);
    }
}