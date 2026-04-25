using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossEnemy : Enemy
{
    [Header("Phases")]
    [SerializeField] float phase2Threshold = 0.6f;
    [SerializeField] float phase3Threshold = 0.3f;

    [Header("Movement")]
    [SerializeField] float phase1Speed = 2.5f;
    [SerializeField] float phase2Speed = 3.5f;
    [SerializeField] float phase3Speed = 4.5f;

    [Header("Radial Burst")]
    [SerializeField] float phase1BurstCooldown = 3f;
    [SerializeField] float phase2BurstCooldown = 2f;
    [SerializeField] float phase3BurstCooldown = 1.5f;

    [Header("Charge")]
    [SerializeField] float chargeCooldown = 6f;
    [SerializeField] float chargeForce = 22f;
    [SerializeField] float chargeDuration = 0.4f;
    [SerializeField] float chargeTelegraphDuration = 1f;

    [Header("Salvo")]
    [SerializeField] float salvoInterval = 0.15f;
    [SerializeField] float salvoCooldown = 4f;
    [SerializeField] Transform[] shootPoints;

    [Header("Summon")]
    [SerializeField] GameObject minionPrefab;
    [SerializeField] Vector3[] minionSpawnPoints = new Vector3[]
    {
        new Vector3(7.5f, 0f, 0f),
        new Vector3(-7.5f, 0f, 0f),
        new Vector3(0f, -7.5f, 0f),
        new Vector3(0f, 7.5f, 0f)
    };
    [SerializeField] int minionCount = 3;
    [SerializeField] float summonCooldown = 8f;
    [SerializeField] int maxSummons = 2;

    [Header("Visual")]
    [SerializeField] Color phase1Color = new Color(0.7f, 0.1f, 0.1f);
    [SerializeField] Color phase2Color = new Color(0.9f, 0.05f, 0.05f);
    [SerializeField] Color phase3Color = new Color(1f, 0f, 0f);
    [SerializeField] Color telegraphColor = Color.yellow;
    [SerializeField] GameObject phaseTransitionParticles;
    [SerializeField] AudioClip bossBeaten;

    SpriteRenderer spriteRenderer;

    int currentPhase = 1;
    bool isCharging;
    int summonsDone;

    float burstTimer;
    float chargeTimer;
    float salvoTimer;
    float summonTimer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        agent.speed = phase1Speed;
        spriteRenderer.color = phase1Color;

        burstTimer = 2f;
        chargeTimer = 4f;
        salvoTimer = 5f;
        summonTimer = summonCooldown;
    }

    protected override void FixedUpdate()
    {
        if (isCharging) return;
        base.FixedUpdate();
    }

    void Update()
    {
        if (isCharging) return;

        burstTimer -= Time.deltaTime;
        chargeTimer -= Time.deltaTime;

        if (currentPhase >= 2)
        {
            salvoTimer -= Time.deltaTime;
            summonTimer -= Time.deltaTime;
        }

        float currentBurstCooldown = currentPhase == 1 ? phase1BurstCooldown
                                   : currentPhase == 2 ? phase2BurstCooldown
                                   : phase3BurstCooldown;

        if (burstTimer <= 0f)
        {
            FireRadialBurst();
            burstTimer = currentBurstCooldown;
        }

        if (chargeTimer <= 0f)
        {
            StartCoroutine(Charge());
            chargeTimer = chargeCooldown;
        }

        if (currentPhase >= 2 && salvoTimer <= 0f)
        {
            StartCoroutine(FireSalvo());
            salvoTimer = salvoCooldown;
        }

        if (currentPhase == 2 && summonTimer <= 0f && summonsDone < maxSummons)
        {
            StartCoroutine(Summon());
            summonTimer = summonCooldown;
        }
    }
    
    void FireRadialBurst()
    {
        foreach (Transform shootPoint in shootPoints)
        {
            SpawnBullet(shootPoint, shootPoint.up);
        }
    }

    IEnumerator FireSalvo()
    {
        foreach (Transform shootPoint in shootPoints)
        {
            Vector2 dir = (target.position - transform.parent.position).normalized;
            SpawnBullet(shootPoint, dir);
            yield return new WaitForSeconds(salvoInterval);
        }
    }

    IEnumerator Charge()
    {
        Color originalColor = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < chargeTelegraphDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 6f, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, telegraphColor, t);
            yield return null;
        }
        spriteRenderer.color = originalColor;

        isCharging = true;
        agent.speed = 0f;
        movementState = MovementState.Free;

        Vector2 chargeDir = (target.position - transform.parent.position).normalized;
        rb.linearVelocity = chargeDir * chargeForce;

        yield return new WaitForSeconds(chargeDuration);

        rb.linearVelocity = Vector2.zero;
        agent.speed = GetCurrentSpeed();
        isCharging = false;
    }

    IEnumerator Summon()
    {
        summonsDone++;
        CameraShake.Instance.Shake(0.2f);
        yield return new WaitForSeconds(0.5f);

        int spawned = 0;
        foreach (Vector3 spawnPoint in minionSpawnPoints)
        {
            if (spawned >= minionCount) break;
            Instantiate(minionPrefab, spawnPoint, Quaternion.identity);
            spawned++;
        }
    }

    void SpawnBullet(Transform shootPoint, Vector2 direction)
    {
        Projectile projectile = GameManager.Instance.ProjectilePool.Get();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.SetPositionAndRotation(shootPoint.position, Quaternion.Euler(0, 0, angle));
        projectile.Init(false);
        projectile.shooterID = gameObject.GetInstanceID();
    }

    public override void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        base.TakeDamage(damage, knockbackDirection, knockbackForce);

        float healthPercent = (float)currentHealth / maxHealth;

        if (currentPhase == 1 && healthPercent <= phase2Threshold)
            EnterPhase(2);
        else if (currentPhase == 2 && healthPercent <= phase3Threshold)
            EnterPhase(3);
    }

    void EnterPhase(int phase)
    {
        currentPhase = phase;
        agent.speed = GetCurrentSpeed();
        spriteRenderer.color = phase == 2 ? phase2Color : phase3Color;

        if (phaseTransitionParticles != null)
            Instantiate(phaseTransitionParticles, transform.parent.position, Quaternion.identity);

        CameraShake.Instance.Shake(0.4f);
    }

    protected override void Die()
    {
        agent.speed = 0f;
        rb.linearVelocity = Vector2.zero;
        
        CameraShake.Instance.Shake(0.8f);
        Instantiate(deathParticleSystem, transform.parent.position, Quaternion.identity);
        
        SoundManager.Instance.PlayClip(bossBeaten, transform.position, 1f);
        Destroy(transform.parent.gameObject);
    }

    float GetCurrentSpeed() => currentPhase switch
    {
        1 => phase1Speed,
        2 => phase2Speed,
        _ => phase3Speed
    };
}