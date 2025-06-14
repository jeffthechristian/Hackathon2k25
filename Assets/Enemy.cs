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
    public float tauntChance = 0.1f; // 30% chance to taunt
    public GameObject meleeHitbox; // Optional: For melee damage

    private Animator animator;
    private NavMeshAgent agent; // Optional: For pathfinding
    private Transform player;
    private float runAttackTimer;
    private float meleeComboTimer;
    private bool isAttacking;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>(); // Optional: Remove if not using NavMesh
        player = GameObject.FindGameObjectWithTag("Player").transform;
        runAttackTimer = runAttackCooldown;
        meleeComboTimer = meleeComboCooldown;

        // Initialize animator parameters
        animator.SetBool("IsPlayerDetected", false);
        animator.SetBool("IsInMeleeRange", false);
    }

    void Update()
    {
        if (isAttacking || health <= 0f) return; // Skip if attacking or dead

        // Update cooldowns
        runAttackTimer -= Time.deltaTime;
        meleeComboTimer -= Time.deltaTime;

        // Detect player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerDetected = distanceToPlayer <= detectionRange && player != null;
        bool inMeleeRange = distanceToPlayer <= meleeRange;

        // Update animator parameters
        animator.SetBool("IsPlayerDetected", playerDetected);
        animator.SetBool("IsInMeleeRange", inMeleeRange);

        if (playerDetected)
        {
            // Face player
            Vector3 dir = (player.position - transform.position).normalized;
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
                else transform.position = transform.position; // No movement

                // Melee combo
                if (meleeComboTimer <= 0f)
                {
                    StartCoroutine(PlayMeleeCombo());
                    meleeComboTimer = meleeComboCooldown;
                }
            }
            else
            {
                // Move toward player
                if (agent)
                {
                    agent.SetDestination(player.position);
                    agent.speed = moveSpeed;
                }
                else
                {
                    transform.position += dir * moveSpeed * Time.deltaTime;
                }

                // Run attack
                if (runAttackTimer <= 0f)
                {
                    StartCoroutine(PlayRunAttack());
                    runAttackTimer = runAttackCooldown;
                }
            }
        }
        else
        {
            // Stop moving
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
        isAttacking = true; // Prevent further actions
        animator.SetTrigger("Die"); // Optional: Play death animation
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        moneyManager.AddMoney(10);
        Destroy(gameObject);
    }

    IEnumerator PlayRunAttack()
    {
        isAttacking = true;
        animator.Play("RunAttack");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false;
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
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false;
    }

    // Called by Animation Events for melee combo or run attack
    void OnAttackHitboxEvent(bool enable)
    {
        if (meleeHitbox) meleeHitbox.SetActive(enable);
    }
}