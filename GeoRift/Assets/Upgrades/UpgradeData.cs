using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public Sprite sprite;
    public int rarity;
    public bool oneTime;
    public StatType statType;
}

public enum StatType { MaxHealth, HealthRegen, MovementSpeed, DashCooldown, ShootCooldown, BulletSpeed, HighCalibre, SMG }
