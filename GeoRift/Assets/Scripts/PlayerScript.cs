using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour, IEntity
{
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
    public bool Shotgun;
    public HealthBar HealthBar;
    
    [SerializeField] float blinkDuration = 0.1f;
    [SerializeField] float blinkInterval = 0.1f;
    [SerializeField] float dashForce = 45f;
    [SerializeField] float dashDuration = 0.5f;
    [SerializeField] Transform shootPoint;
    [SerializeField] GameObject deathParticleSystem;
    [SerializeField] Sprite whiteSprite;
    [SerializeField] AudioClip playerShoot;
    [SerializeField] AudioClip playerHit;
    [SerializeField] AudioClip playerDeath;

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
        SoundManager.Instance.PlayClip(playerHit, tf.position, 1f, Random.Range(0.8f, 1.2f));
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        
        HealthBar.UpdateHealthBar(currentHealth);
    }
    
    public void Heal()
    {
        currentHealth = MaxHealth;
        HealthBar.UpdateHealthBar(currentHealth);
    }

    void Die()
    {
        Instantiate(deathParticleSystem, transform.position, Quaternion.identity);
        
        MenuManager.Instance.ShowGameOver();
        SoundManager.Instance.PlayClip(playerDeath, tf.position, 1f);
        
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
        
        int projectileAmount = Shotgun ? 5 : 1;
        // Instantiating projectiles on shoot
        for (int i = 0; i < projectileAmount; i++)
        {
            Projectile projectile = GameManager.Instance.ProjectilePool.Get();
            projectile.transform.SetPositionAndRotation(shootPoint.position, shootPoint.rotation);
            projectile.Init(true);
            projectile.shooterID = gameObject.GetInstanceID();
        }
        SoundManager.Instance.PlayClip(playerShoot, tf.position, 0.5f, Random.Range(0.7f, 1.3f));
        shootTimer = 0f;
    }
}
