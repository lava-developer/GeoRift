using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IEntity
{
    public float KnockbackForce { get; } = 25f;
    public int AttackDamage;
    public bool Immune { get; private set; }

    [SerializeField] protected int maxHealth = 100;
    [SerializeField] float knockbackModifier = 1f;
    [SerializeField] float immunityDuration = 1f;
    [SerializeField] float blinkDuration = 0.1f;
    [SerializeField] float blinkInterval = 0.1f;
    [SerializeField] GameObject deathParticleSystem;
    [SerializeField] bool hasHealthBar = true;

    protected int currentHealth;
    protected Rigidbody2D rb;
    protected NavMeshAgent agent;
    protected Transform target;
    protected float movementSpeed;
    protected MovementState movementState = MovementState.Free;

    Material material;
    HealthBar healthBar;
    protected EnemySpawner spawner;
    Coroutine blinkCoroutine;

    protected enum MovementState { Free, Knocked }

    protected virtual void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        agent = GetComponentInParent<NavMeshAgent>();
        material = GetComponent<Renderer>().material;
    }

    protected virtual void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>();
        maxHealth = Mathf.RoundToInt(maxHealth * GameManager.EnemyHealthModifier);
        currentHealth = maxHealth;

        if (hasHealthBar)
        {
            healthBar = transform.parent.GetComponentInChildren<HealthBar>();
            healthBar.InitializeHealthBar(maxHealth);
            healthBar.UpdateHealthBar(currentHealth);
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        movementSpeed = agent.speed;
        target = GameManager.Instance.Player.transform;
        Immune = false;
    }

    protected virtual void FixedUpdate()
    {
        switch (movementState)
        {
            case MovementState.Free:
                agent.SetDestination(target.position);
                break;

            case MovementState.Knocked:
                if (rb.linearVelocity.magnitude < 0.5f)
                {
                    rb.linearVelocity = Vector2.zero;
                    movementState = MovementState.Free;
                    agent.speed = movementSpeed;
                }
                break;
        }
    }

    public void Knockback(Vector2 direction, float force)
    {
        agent.speed = 0f;
        movementState = MovementState.Knocked;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force * knockbackModifier, ForceMode2D.Impulse);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(Blinking());
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }
        if (hasHealthBar)
            healthBar.UpdateHealthBar(currentHealth);
    }

    protected virtual void Die()
    {
        Instantiate(deathParticleSystem, transform.position, Quaternion.identity);
        spawner.EnemyDeath();
        Destroy(transform.parent.gameObject);
    }

    IEnumerator Blinking()
    {
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
}