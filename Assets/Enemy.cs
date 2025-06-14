using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Optional: For NavMesh movement

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public MoneyManager moneyManager;
    public float detectionRange = 15f;
    public float meleeRange = 2f;
    public float moveSpeed = 5f;
    public float runAttackCooldown = 2f;
    public float meleeComboCooldown = 3f;
    public float tauntChance = 0.1f; 
    public GameObject meleeHitbox; 
    public AudioClip tauntAudioClip; 
    public float tauntVolume = 1f; 

    private Animator animator;
    private NavMeshAgent agent; 
    private Transform target;
    private AudioSource audioSource; 
    private float runAttackTimer;
    private float meleeComboTimer;
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
        runAttackTimer = runAttackCooldown;
        meleeComboTimer = meleeComboCooldown;

        animator.SetBool("IsTargetDetected", false);
        animator.SetBool("IsInMeleeRange", false);
    }

    void Update()
    {
        if (isAttacking || health <= 0f) return; // Skip if attacking or dead

        // Update cooldowns
        runAttackTimer -= Time.deltaTime;
        meleeComboTimer -= Time.deltaTime;

        // Detect target
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        bool targetDetected = distanceToTarget <= detectionRange && target != null;
        bool inMeleeRange = distanceToTarget <= meleeRange;

        // Update animator parameters
        animator.SetBool("IsTargetDetected", targetDetected);
        animator.SetBool("IsInMeleeRange", inMeleeRange);

        if (targetDetected)
        {
            // Face target
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

            // Random taunt when not in melee range
            if (Random.value < tauntChance && !inMeleeRange && runAttackTimer <= 0f)
            {
                StartCoroutine(PlayTaunt());
                return;
            }

            if (inMeleeRange)
            {
                // Stop moving
                if (agent) agent.SetDestination(transform.position);
                else transform.position = transform.position;

                // Melee combo
                if (meleeComboTimer <= 0f)
                {
                    StartCoroutine(PlayMeleeCombo());
                    meleeComboTimer = meleeComboCooldown;
                }
            }
            else
            {
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
        else
        {
            if (agent) agent.SetDestination(transform.position);
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

    IEnumerator PlayMeleeCombo()
    {
        isAttacking = true;
        animator.Play("MeleeCombo");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
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