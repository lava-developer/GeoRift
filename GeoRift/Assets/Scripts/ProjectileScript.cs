using UnityEditor.Callbacks;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public float ProjectileSpeed = 20f;
    public float Lifetime = 15f;

    Rigidbody2D projectileRigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Apply velocity to projectile
        projectileRigidbody = GetComponent<Rigidbody2D>();
        projectileRigidbody.linearVelocity = transform.up * ProjectileSpeed;

        // Destroy projectile after its lifetime expires
        Destroy(gameObject, Lifetime);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Environment")
        {
            Destroy(gameObject);
        }
    }
}
