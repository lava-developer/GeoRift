using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class BasicEnemy : MonoBehaviour, IEnemy
{
    public float KnockbackForce {get; } = 25f;
    public int AttackDamage {get; } = 34;

    [SerializeField] int maxHealth = 100;
    int currentHealth;
    [SerializeField] float immunityDuration = 0.5f;
    [SerializeField] float blinkDuration = 0.1f;
    [SerializeField] float blinkInterval = 0.1f;

    [SerializeField] GameObject deathParticleSystem;
    [SerializeField] HealthBar healthBar;

    Rigidbody2D rb;
    SpriteRenderer sr;
    NavMeshAgent agent;

    Transform target;
    float movementSpeed;
    MovementState movementState = MovementState.Free;
    Coroutine blinkCoroutine;
    Color spriteColor;

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        
        currentHealth = maxHealth;
        healthBar.InitializeHealthBar(maxHealth);

        spriteColor = sr.color;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        movementSpeed = agent.speed;
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        switch (movementState)
        {
            case MovementState.Free:
                agent.SetDestination(target.position);
                break;
            
            case MovementState.Knocked:
                // Check if knockback has ended and if so return to free movement
                if (rb.linearVelocity.magnitude < 0.5f)
                {
                    rb.linearVelocity = Vector2.zero;
                    movementState = MovementState.Free;
                    agent.speed = movementSpeed;
                }
                break;
        }
    }

    enum MovementState
    {
        Free,
        Knocked,
    }

    IEnumerator Blinking()
    {
        // Blink sprite white while knocked back
        sr.color = Color.white;

        int blinkAmount = Mathf.CeilToInt(immunityDuration / (blinkDuration + blinkInterval));

        for (int i = 0; i < blinkAmount; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(blinkDuration);
            sr.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }
        sr.color = spriteColor;
    }

    public void Knockback(Vector2 direction, float force)
    {
        // Apply knockback
        agent.speed = 0f;
        movementState = MovementState.Knocked;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine = StartCoroutine(Blinking());
    }

    public void TakeDamage(int damage)
    {
        // Apply damage and check for death
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;          
            Die();
        }
        healthBar.UpdateHealthBar(currentHealth);
    }

    void Die()
    {
        Instantiate(deathParticleSystem, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
