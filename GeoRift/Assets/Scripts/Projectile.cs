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

    void Awake()
    {
        pool = GameManager.Instance.ProjectilePool;
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
    }

    public void Init(bool isPlayers)
    {
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
            knockbackForce = 15;
            projectileSpeed = 20;
            spray = 30;
        }

        float randomAngle = Random.Range(-spray, spray);
        transform.Rotate(0f, 0f, randomAngle, Space.Self);
        
        tr.Clear();
        tr.emitting = true;
        // Apply velocity to projectile
        rb.linearVelocity = transform.up.normalized * projectileSpeed;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {   
        // If hit an enemy deal damage and destroy projectile
        if ((collision.gameObject.CompareTag("Enemy") ||  collision.gameObject.CompareTag("Player")) && collision.transform.GetChild(0).gameObject.GetInstanceID() != shooterID)
        {
            IEntity entity = collision.gameObject.GetComponentInChildren<IEntity>();
            if (!entity.Immune)
            {
                entity.Knockback(rb.linearVelocity.normalized, knockbackForce);
                entity.TakeDamage(damage);
            }
            tr.emitting = false;
            pool.Release(this);
            return;
        }
        else if (collision.gameObject.CompareTag("Environment"))
        {
            tr.emitting = false;
            pool.Release(this);
            return;
        }
    }
}
