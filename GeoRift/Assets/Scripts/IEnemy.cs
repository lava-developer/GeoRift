using UnityEngine;

public interface IEnemy
{
    void Knockback(Vector2 direction, float force);
    void TakeDamage(int damage);
    
    float KnockbackForce {get; }
    int AttackDamage {get; }
}
