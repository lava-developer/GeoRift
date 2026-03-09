using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int shooterID;
    
    [SerializeField] float knockbackForce = 15f;
    [SerializeField] int damage = 34;
    [SerializeField] float projectileSpeed = 20f;
    [SerializeField] float lifetime = 15f;

    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Apply velocity to projectile
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * projectileSpeed;

        // Destroy projectile after its lifetime expires
        Destroy(gameObject, lifetime);
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
            Destroy(gameObject);
        }
        // If hit environment just destroy projectile
        else if (collision.gameObject.CompareTag("Environment"))
        {
            Destroy(gameObject);
        }
    }
}
