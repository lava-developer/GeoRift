using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class PlayerScript : MonoBehaviour, IEntity
{
    public ObjectPool<Projectile> ProjectilePool;
    public bool Immune { get; private set; }

    public int MaxHealth;
    int currentHealth;
    public int HealthRegen;
    public float MovementSpeed;
    public float DashCooldown;
    public float KnockbackForce { get; set; }
    public float ShootCooldown;
    public float ImmunityDuration;
    public bool AutoFire;
    public HealthBar HealthBar;
    
    [SerializeField] float blinkDuration = 0.1f;
    [SerializeField] float blinkInterval = 0.1f;
    [SerializeField] float dashForce = 20f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] Transform shootPoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject deathParticleSystem;
    [SerializeField] Sprite whiteSprite;

    Transform tf;
    Rigidbody2D rb;
    Camera cam;
    TrailRenderer dashTrail;
    PlayerInput input;
    Vector2 movementInput;
    Material material;
    float dashTimer;
    float shootTimer;
    MovementState movementState = MovementState.Free;

    enum MovementState
    {
        Free,
        Knocked,
        Dashing,
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
        dashTrail = GetComponent<TrailRenderer>();
        input = GetComponent<PlayerInput>();

        material = GetComponent<Renderer>().material;

        ProjectilePool = new ObjectPool<Projectile>(
        createFunc: () => Instantiate(projectilePrefab).GetComponent<Projectile>(),
        actionOnGet: p => p.gameObject.SetActive(true),
        actionOnRelease: p => p.gameObject.SetActive(false),
        actionOnDestroy: p => Destroy(p.gameObject),
        defaultCapacity: 20,
        maxSize: 50
        );

        // Bind input actions
        input.actions["Aim"].performed += OnAim;
        input.actions["Dash"].performed += OnDash;

        currentHealth = MaxHealth;
        HealthBar.InitializeHealthBar(MaxHealth);
        HealthBar.UpdateHealthBar(currentHealth);

        InvokeRepeating(nameof(HealthRegenFunc), 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        // Get movement input from input
        movementInput = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        
        dashTimer -= Time.deltaTime;
        shootTimer += Time.deltaTime;

        bool shootPressed = AutoFire ? input.actions["Shoot"].IsPressed() : input.actions["Shoot"].WasPressedThisFrame();

        if (shootPressed)
            TryShoot();
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

    void HealthRegenFunc()
    {
        currentHealth += HealthRegen;
        if (currentHealth > MaxHealth)
            currentHealth = MaxHealth;
        HealthBar.UpdateHealthBar(currentHealth);
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

    void OnDash(InputAction.CallbackContext context)
    {
        if (dashTimer > 0 || movementState == MovementState.Dashing) return;

        StartCoroutine(DashCoroutine());
    }

    IEnumerator DashCoroutine()
    {
        movementState = MovementState.Dashing;
        dashTrail.emitting = true;
        
        Vector2 dir = movementInput.sqrMagnitude > 0 ? movementInput.normalized : (Vector2)tf.up;
        rb.linearVelocity = dir * dashForce;
        
        yield return new WaitForSeconds(dashDuration);
        
        movementState = MovementState.Free;
        dashTrail.emitting = false;
        dashTimer = DashCooldown;
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

    void TryShoot()
    {
        if (shootTimer < ShootCooldown) return;
        
        // Instantiating projectile on shoot
        Projectile projectile = ProjectilePool.Get();
        projectile.transform.SetPositionAndRotation(shootPoint.position, shootPoint.rotation);
        projectile.Init();
        projectile.shooterID = gameObject.GetInstanceID();
        shootTimer = 0f;
    }
}
