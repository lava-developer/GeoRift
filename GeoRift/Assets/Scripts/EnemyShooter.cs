using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] Transform[] shootPoints;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float shootInterval = 2f;

    [SerializeField] LayerMask obstacleMask;

    Transform tf;
    Transform target;

    float shootTimer;

    void Start()
    {
        tf = transform;
        target = GameManager.Instance.Player.transform;
    }

    void Update()
    {
        if (hasLineOfSight())
        {
            //tf.rotation = Quaternion.LookRotation(Vector3.forward, target.position - tf.position);
            Vector3 dir = target.position - tf.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            tf.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            tf.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
        
    }

    bool hasLineOfSight()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 50f, obstacleMask);
        return hit && hit.collider.CompareTag("Player");
    }

    void Shoot()
    {
        foreach (Transform shootPoint in shootPoints)
        {
            Projectile projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation).GetComponent<Projectile>();
            projectile.shooterID = gameObject.GetInstanceID();
        }
    }
}
