using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    public int shooterID;

    bool isPlayerProjectile;
    float knockbackForce = 15f;
    int damage = 34;
    float projectileSpeed = 20f;
    float spray = 10f;

    ObjectPool<Projectile> pool;
    Rigidbody2D rb;
    TrailRenderer tr;

    float age = 0f;
    bool isReleased = true;

    void Awake()
    {
        pool = GameManager.Instance.ProjectilePool;
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
    }

    public void Init(bool isPlayers)
    {
        isReleased = false;
        isPlayerProjectile = isPlayers;
        rb.linearVelocity = Vector2.zero;

        if (isPlayerProjectile)
        {
            damage = PlayerStatistics.BulletDamage;
            knockbackForce = PlayerStatistics.BulletKnockbackForce;
            projectileSpeed = PlayerStatistics.BulletSpeed;
            spray = PlayerStatistics.Spray;
        }
        else
        {
            damage = 12;
            knockbackForce = 15f;
            projectileSpeed = 20f;
            spray = 20f;
        }

        float randomAngle = Random.Range(-spray, spray);
        transform.Rotate(0f, 0f, randomAngle, Space.Self);

        tr.Clear();
        tr.emitting = true;
        // Apply velocity to projectile
        rb.linearVelocity = transform.up.normalized * projectileSpeed;
        age = 0f;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age > 5f)
        {
            ReleaseToPool();
        }
    }

    void ReleaseToPool()
    {
        if (isReleased) return;
        isReleased = true;
        tr.emitting = false;
        pool.Release(this);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile") && age < 0.1f)
            return;
        else if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player"))
        {
            if (collision.transform.GetChild(0).gameObject.GetInstanceID() == shooterID)
                return;
            else
            {
                IEntity entity = collision.gameObject.GetComponentInChildren<IEntity>();
                entity.TakeDamage(damage, rb.linearVelocity.normalized, knockbackForce);
                tr.emitting = false;
                ReleaseToPool();
                return;
            }
        }
        else
        {
            tr.emitting = false;
            ReleaseToPool();
            return;
        }
    }
}
