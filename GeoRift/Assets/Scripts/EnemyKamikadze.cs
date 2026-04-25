using UnityEngine;
using UnityEngine.AI;

public class KamikazeEnemy : Enemy
{
    [Header("Kamikaze")]
    [SerializeField] float directionUpdateInterval = 0.12f;
    [SerializeField] float explosionRadius = 2.5f;
    [SerializeField] int explosionDamage = 40;
    [SerializeField] float triggerDistance = 1f;
    [SerializeField] float warningDistance = 3f;
    [SerializeField] Color normalColor = new Color(1f, 0.6f, 0f);
    [SerializeField] Color warningColor = Color.red;
    [SerializeField] GameObject explosionParticleSystem;
    [SerializeField] AudioClip explosion;

    SpriteRenderer spriteRenderer;
    float directionTimer;
    bool exploded;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        if (exploded) return;

        directionTimer -= Time.fixedDeltaTime;
        if (directionTimer <= 0f)
        {
            agent.SetDestination(target.position);
            directionTimer = directionUpdateInterval;
        }

        if (movementState == MovementState.Knocked)
        {
            if (rb.linearVelocity.magnitude < 0.5f)
            {
                rb.linearVelocity = Vector2.zero;
                movementState = MovementState.Free;
                agent.speed = movementSpeed;
            }
            return;
        }

        float dist = Vector2.Distance(transform.parent.position, target.position);
        float t = 1f - Mathf.Clamp01((dist - triggerDistance) / warningDistance);
        spriteRenderer.color = Color.Lerp(normalColor, warningColor, t);

        if (dist <= triggerDistance)
            Explode();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Explode();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
            Explode();
    }

    public override void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        if (exploded) return;
        Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Enemy"))
                hit.GetComponent<IEntity>().TakeDamage(explosionDamage, (hit.transform.position - transform.position).normalized, 30f);
        }

        Instantiate(explosionParticleSystem, transform.position, Quaternion.identity);
        CameraShake.Instance.Shake(0.5f);
        SoundManager.Instance.PlayClip(explosion, transform.position, 0.75f);
        Destroy(transform.parent.gameObject);
    }
}