using UnityEngine;

public interface IEntity
{
    void Knockback(Vector2 direction, float force);
    void TakeDamage(int damage);
    
    float KnockbackForce {get; }
    bool Immune {get; }
}