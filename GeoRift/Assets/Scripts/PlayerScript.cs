using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    int currentHealth;
    [SerializeField] float movementSpeed = 10f;
    [SerializeField] float playerKnockbackForce = 30f;
    [SerializeField] float blinkDuration = 0.1f;
    [SerializeField] float blinkInterval = 0.1f;

    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject deathParticleSystem;
    [SerializeField] HealthBar healthBar;
    [SerializeField] Volume volume;
    [SerializeField] Color deathVignetteColor;
    [SerializeField] float deathVignetteIntensity = 0.6f;

    Transform tf;
    Rigidbody2D rb;
    Camera cam;
    PlayerInput input;
    SpriteRenderer sr;
    
    Vector2 movementInput;
    MovementState movementState = MovementState.Free;
    Vignette vignette;
    Color vignetteOriginalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize components
        tf = transform;
        rb = GetComponentInParent<Rigidbody2D>();
        cam = Camera.main;
        input = GetComponent<PlayerInput>();
        sr = GetComponent<SpriteRenderer>();

        volume.profile.TryGet<Vignette>(out vignette);
        vignetteOriginalColor = vignette.color.value;

        // Bind input actions
        input.actions["Aim"].performed += OnAim;
        input.actions["Shoot"].performed += OnShoot;

        currentHealth = maxHealth;
        healthBar.InitializeHealthBar(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        // Get movement input from input
        movementInput = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Move player based on input
        switch (movementState)
        {
            case MovementState.Free:
                rb.linearVelocity = movementInput.normalized * movementSpeed;
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
            IEnemy enemy = collision.gameObject.GetComponent<IEnemy>();
            enemy.Knockback((collision.gameObject.transform.position - tf.position).normalized, playerKnockbackForce);
            TakeDamage(enemy.AttackDamage, (tf.position - collision.gameObject.transform.position).normalized, enemy.KnockbackForce);
        }
    }

    enum MovementState
    {
        Free,
        Knocked,
    }

    IEnumerator Blinking()
    {
        sr.color = Color.white;
        while (movementState == MovementState.Knocked)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(blinkDuration);
            sr.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }
        sr.color = new Color(0.2169999f, 0.7369609f, 1f, 1f);
    }

    void Knockback(Vector2 direction, float force)
    {
        movementState = MovementState.Knocked;

        // Apply knockback
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        StartCoroutine(Blinking());
    }

    void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        currentHealth -= damage;
        float t = currentHealth / (float)maxHealth;
        t = Mathf.Clamp01(t);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
            Knockback(knockbackDirection, knockbackForce);
        
        healthBar.UpdateHealthBar(currentHealth);
        vignette.color.value = Color.Lerp(deathVignetteColor, vignetteOriginalColor, t);
        vignette.intensity.value = Mathf.Lerp(deathVignetteIntensity, 0.1f, t);
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
        // Instantiating projectile on shoot
        GameObject projectile = Instantiate(projectilePrefab, tf.position + tf.up * 0.58f, tf.rotation);
    }
}
