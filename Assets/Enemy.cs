using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Optional: For NavMesh movement

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public MoneyManager moneyManager;
    public float meleeRange = 2f;
    public float moveSpeed = 5f;
    public float attackCooldown = 2f; // Cooldown between attacks
    public float tauntCooldown = 5f; // Cooldown between taunts
    public float tauntChance = 0.2f; // 20% chance to taunt after attack
    public GameObject meleeHitbox;
    public AudioClip tauntAudioClip;
    public float tauntVolume = 1f;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform target;
    private AudioSource audioSource;
    private float attackTimer;
    private float tauntTimer;
    private bool isAttacking;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
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

        if (health <= 0)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        isAttacking = true;
        animator.SetTrigger("Die");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        moneyManager.AddMoney(10);
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
        if (tauntAudioClip != null)
        {
            audioSource.PlayOneShot(tauntAudioClip, tauntVolume);
        }
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false;
    }

    void OnAttackHitboxEvent(bool enable)
    {
        if (meleeHitbox) meleeHitbox.SetActive(enable);
    }
}