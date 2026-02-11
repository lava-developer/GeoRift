using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float shootInterval = 2f;

    [SerializeField] LayerMask obstacleMask;

    Transform tf;

    void Start()
    {
        tf = transform;
    }

    void Update()
    {
        if (hasLineOfSight())
        {
            tf.rotation = Quaternion.LookRotation(Vector3.forward, player.position - tf.position);
        }
    }

    bool hasLineOfSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 50f, obstacleMask);
        return hit && hit.collider.CompareTag("Player");
    }

}
