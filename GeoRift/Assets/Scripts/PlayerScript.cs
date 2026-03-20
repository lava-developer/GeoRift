using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour, IEntity
{
    public bool Immune { get; private set; }

    public int MaxHealth;
    int currentHealth;
    public float MovementSpeed;
    public float KnockbackForce { get; set; }
    public float ShootCooldown;
    public float ImmunityDuration;
    public HealthBar HealthBar;
    
    [SerializeField] float blinkDuration = 0.1f;
    [SerializeField] float blinkInterval = 0.1f;
    [SerializeField] Transform shootPoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject deathParticleSystem;
    [SerializeField] Sprite whiteSprite;

    Transform tf;
    Rigidbody2D rb;
    Camera cam;
    PlayerInput input;
    Vector2 movementInput;
    Material material;
    float shootTimer;
    MovementState movementState = MovementState.Free;

    enum MovementState
    {
        Free,
        Knocked,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Load player stats from GameManager
        PlayerStatistics.UpdatePlayerStats();

        // Initialize components
        tf = transform;
        rb = GetComponentInParent<Rigidbody2D>();
        cam = Camera.main;
        input = GetComponent<PlayerInput>();

        material = GetComponent<Renderer>().material;

        // Bind input actions
        input.actions["Aim"].performed += OnAim;
        input.actions["Shoot"].performed += OnShoot;

        currentHealth = MaxHealth;
        HealthBar.InitializeHealthBar(MaxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        // Get movement input from input
        movementInput = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();

        shootTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Move player based on input
        switch (movementState)
        {
            case MovementState.Free:
                rb.linearVelocity = movementInput.normalized * MovementSpeed;
                break;
            
            case MovementState.Knocked:
                if (rb.linearVelocity.magnitude < 0.5f)
                {
                    rb.linearVelocity = Vector2.zero;
                    movementState = MovementState.Free;
                }
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Take damage when colliding with an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponentInChildren<Enemy>();
            if (!enemy.Immune)
                enemy.Knockback((collision.gameObject.transform.position - tf.position).normalized, KnockbackForce);
            if (!Immune)
            {
                Knockback((tf.position - collision.gameObject.transform.position).normalized, enemy.KnockbackForce);
                TakeDamage(enemy.AttackDamage);
            }
        }
    }

    IEnumerator Blinking()
    {
        // Blink sprite white while knocked back

        Immune = true;
        int blinkAmount = Mathf.CeilToInt(ImmunityDuration / (blinkDuration + blinkInterval));

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
        movementState = MovementState.Knocked;

        // Apply knockback
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        StartCoroutine(Blinking());
    }

   public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        float t = currentHealth / (float)MaxHealth;
        t = Mathf.Clamp01(t);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        
        HealthBar.UpdateHealthBar(currentHealth);
    }

    void Die()
    {
        Instantiate(deathParticleSystem, transform.position, Quaternion.identity);

        tf.parent.gameObject.SetActive(false);
    }

    void OnAim(InputAction.CallbackContext context)
    {
        // Rotate player to face the cursor
        Vector3 mousePos = cam.ScreenToWorldPoint(context.ReadValue<Vector2>());
        mousePos.z = 0;

        Vector3 direction = mousePos - tf.position;
        float cameraAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

        tf.rotation = Quaternion.Euler(0, 0, cameraAngle);
    }

    void OnShoot(InputAction.CallbackContext context)
    {
        if (shootTimer < ShootCooldown) return;
        
        // Instantiating projectile on shoot
        Projectile projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation).GetComponent<Projectile>();
        projectile.shooterID = gameObject.GetInstanceID();
        shootTimer = 0f;
    }
}
