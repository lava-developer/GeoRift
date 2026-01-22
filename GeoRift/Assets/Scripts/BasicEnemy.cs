using UnityEngine;
using System.Collections;

public class BasicEnemy : MonoBehaviour, IEnemy
{
    public float KnockbackForce {get; } = 25f;
    public int AttackDamage {get; } = 34;

    [SerializeField] int maxHealth = 100;
    int currentHealth;
    [SerializeField] float blinkDuration = 0.1f;
    [SerializeField] float blinkInterval = 0.1f;

    [SerializeField] GameObject deathParticleSystem;

    Rigidbody2D rb;
    SpriteRenderer sr;

    MovementState movementState = MovementState.Free;

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    void FixedUpdate()
    {
        switch (movementState)
        {
            case MovementState.Free:
                // Can move
            
            case MovementState.Knocked:
                // Check if knockback has ended and if so return to free movement
                if (rb.linearVelocity.magnitude < 0.5f)
                {
                    rb.linearVelocity = Vector2.zero;
                    movementState = MovementState.Free;
                }
                break;
        }
    }

    void Die()
    {
        Instantiate(deathParticleSystem, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    enum MovementState
    {
        Free,
        Knocked,
    }

    IEnumerator Blinking()
    {
        // Blink sprite white while knocked back
        Color color = sr.color;
        sr.color = Color.white;
        while (movementState == MovementState.Knocked)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(blinkDuration);
            sr.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }
        sr.color = color;
    }

    public void Knockback(Vector2 direction, float force)
    {
        // Apply knockback
        movementState = MovementState.Knocked;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        StartCoroutine(Blinking());
    }

    public void TakeDamage(int damage)
    {
        // Apply damage and check for death
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
}
