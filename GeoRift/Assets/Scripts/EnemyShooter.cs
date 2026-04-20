using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] Transform[] shootPoints;
    [SerializeField] float shootInterval = 2f;

    [SerializeField] LayerMask obstacleMask;

    Transform tf;
    Rigidbody2D rb;
    Transform target;

    float shootTimer;

    void Start()
    {
        tf = transform;
        rb = tf.parent.GetComponent<Rigidbody2D>();
        target = GameManager.Instance.Player.transform;
    }

    void Update()
    {
        Vector3 dir = target.position - tf.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        tf.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        tf.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

        shootTimer += Time.deltaTime;

        if (hasLineOfSight())
        {
            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
    }

    bool hasLineOfSight()
    {
        // Check if there are any obstacles between the enemy and the player
        Vector2 direction = (target.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 50f, obstacleMask);
        return hit && hit.collider.CompareTag("Player");
    }

    void Shoot()
    {
        foreach (Transform shootPoint in shootPoints)
        {
            Projectile projectile = GameManager.Instance.ProjectilePool.Get();
            projectile.transform.SetPositionAndRotation(shootPoint.position, shootPoint.rotation);
            projectile.Init(false);
            projectile.shooterID = gameObject.GetInstanceID();
        }
    }
}
