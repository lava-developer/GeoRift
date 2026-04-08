using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    public int shooterID;
    
    [SerializeField] bool isPlayerProjectile;
    [SerializeField] float knockbackForce = 15f;
    [SerializeField] int damage = 34;
    [SerializeField] float projectileSpeed = 20f;
    [SerializeField] float spray = 10f;

    ObjectPool<Projectile> pool;
    Rigidbody2D rb;

    void Awake()
    {
        pool = GameManager.Instance.Player.GetComponent<PlayerScript>().ProjectilePool;
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init()
    {
        rb.linearVelocity = Vector2.zero;

        if (isPlayerProjectile)
        {
            damage = PlayerStatistics.BulletDamage;
            projectileSpeed = PlayerStatistics.BulletSpeed;
            spray = PlayerStatistics.Spray;
        }

        float randomAngle = Random.Range(-spray, spray);
        transform.Rotate(0f, 0f, randomAngle, Space.Self);

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
            pool.Release(this);
            return;
        }
        else if (collision.gameObject.CompareTag("Environment"))
        {
            pool.Release(this);
            return;
        }
    }
}
