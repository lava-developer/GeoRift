using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public Sprite sprite;
    public int rarity;

    public StatType statType;
    public float value;
}

public enum StatType { MaxHealth, MovementSpeed, ProjectileDamage, ShootCooldown, ImmunityDuration, KnockbackForce }
