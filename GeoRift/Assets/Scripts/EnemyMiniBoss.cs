using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MiniBossEnemy : Enemy
{
    [Header("Mini Boss")]
    [SerializeField] float phase2SpeedMultiplier = 1.6f;
    [SerializeField] float dashForce = 18f;
    [SerializeField] float dashCooldown = 3f;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] int phase2HealthThreshold = 50;

    [Header("Visual")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Color phase1Color = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] Color phase2Color = new Color(1f, 0.1f, 0.1f);
    [SerializeField] GameObject explosionParticleSystem;
    [SerializeField] float explosionRadius = 2f;
    [SerializeField] int explosionDamage = 30;
    [SerializeField] AudioClip explosion;
    [SerializeField] AudioClip bossBeaten;

    bool isPhase2;
    bool isDashing;
    float dashTimer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        spriteRenderer.color = phase1Color;
        dashTimer = dashCooldown;
    }

    protected override void FixedUpdate()
    {
        if (isDashing) return;

        base.FixedUpdate();

        if (!isPhase2) return;

        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            StartCoroutine(Dash());
            dashTimer = dashCooldown;
        }
    }

    public override void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        base.TakeDamage(damage, knockbackDirection, knockbackForce);

        float healthPercent = (float)currentHealth / maxHealth * 100f;
        if (!isPhase2 && healthPercent <= phase2HealthThreshold)
            EnterPhase2();
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        agent.speed = movementSpeed * phase2SpeedMultiplier;
        spriteRenderer.color = phase2Color;

        Explode();
    }

    IEnumerator Dash()
    {
        isDashing = true;
        agent.speed = 0f;
        movementState = MovementState.Knocked;

        Vector2 direction = (target.position - transform.parent.position).normalized;
        rb.linearVelocity = direction * dashForce;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        movementState = MovementState.Free;
        agent.speed = movementSpeed * phase2SpeedMultiplier;
        isDashing = false;
    }
    
    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
                hit.GetComponent<PlayerScript>().TakeDamage(explosionDamage, (hit.transform.position - transform.position).normalized, 30f);
        }

        Instantiate(explosionParticleSystem, transform.position, Quaternion.identity);
        CameraShake.Instance.Shake(0.5f);
        SoundManager.Instance.PlayClip(explosion, transform.position, 0.75f);
    }

    protected override void Die()
    {
        Explode();
        SoundManager.Instance.PlayClip(bossBeaten, transform.position, 1f);
        Destroy(transform.parent.gameObject);
    }
}