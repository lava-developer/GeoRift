using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IEntity
{
    public float KnockbackForce { get; } = 25f;
    public int AttackDamage;
    public bool Immune { get; private set; }

    [SerializeField] int maxHealth = 100;
    int currentHealth;
    [SerializeField] float knockbackModifier = 1f;
    [SerializeField] float immunityDuration = 1f;
    [SerializeField] float blinkDuration = 0.1f;
    [SerializeField] float blinkInterval = 0.1f;

    [SerializeField] GameObject deathParticleSystem;
    [SerializeField] Sprite whiteSprite;

    Rigidbody2D rb;
    Material material;
    HealthBar healthBar;
    NavMeshAgent agent;
    EnemySpawner spawner;

    Transform target;
    float movementSpeed;
    MovementState movementState = MovementState.Free;
    Coroutine blinkCoroutine;

    void Start()
    
    {
        // Initialize components
        rb = GetComponentInParent<Rigidbody2D>();
        agent = GetComponentInParent<NavMeshAgent>();
        spawner = FindFirstObjectByType<EnemySpawner>();

        maxHealth = Mathf.RoundToInt(maxHealth * GameManager.EnemyHealthModifier);
        currentHealth = maxHealth;
        healthBar = transform.parent.GetComponentInChildren<HealthBar>();
        healthBar.InitializeHealthBar(maxHealth);
        Immune = false;

        material = GetComponent<Renderer>().material;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        movementSpeed = agent.speed;
        target = GameManager.Instance.Player.transform;
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

        Immune = true;
        int blinkAmount = Mathf.CeilToInt(immunityDuration / (blinkDuration + blinkInterval));

        for (int i = 0; i < blinkAmount; i++)
        {
            material.SetFloat("_White", 1f);
            yield return new WaitForSeconds(blinkDuration);
            material.SetFloat("_White", 0f);
            yield return new WaitForSeconds(blinkInterval);
        }
        Immune = false;
    }

    public void Knockback(Vector2 direction, float force)
    {
        // Apply knockback
        agent.speed = 0f;
        movementState = MovementState.Knocked;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force * knockbackModifier, ForceMode2D.Impulse);

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
        spawner.EnemyDeath();
        Destroy(transform.parent.gameObject);
    }
}
