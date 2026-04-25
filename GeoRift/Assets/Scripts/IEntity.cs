using UnityEngine;

public interface IEntity
{
    void Knockback(Vector2 direction, float force);
    void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce);
    float KnockbackForce {get; }
}