using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            IEnemy enemy = collision.gameObject.GetComponent<IEnemy>();
            enemy.TakeDamage(damage);
            enemy.Knockback(rb.linearVelocity.normalized, knockbackForce);
            Destroy(gameObject);
        }
        // If hit environment just destroy projectile
        else if (collision.gameObject.CompareTag("Environment"))
        {
            Destroy(gameObject);
        }
    }
}
