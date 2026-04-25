using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Pool;

public static class PlayerStatistics
{
    public static int MaxHealth = 100;
    public static int HealthRegen = 0;
    public static float MovementSpeed = 3f;
    public static float DashCooldown = 1f;
    public static float KnockbackForce = 30f;
    public static float ShootCooldown = 0.5f;
    public static float ImmunityDuration = 0.25f;
    public static int BulletDamage = 20;
    public static float BulletKnockbackForce = 15f;
    public static float BulletSpeed = 15f;
    public static float Spray = 5f;
    public static bool AutoFire = false;
    public static bool Shotgun = false;

    public static void UpdatePlayerStats()
    {
        PlayerScript player = GameManager.Instance.Player.GetComponent<PlayerScript>();

        player.MaxHealth = MaxHealth;
        player.HealthRegen = HealthRegen;
        player.MovementSpeed = MovementSpeed;
        player.DashCooldown = DashCooldown;
        player.KnockbackForce = KnockbackForce;
        player.ShootCooldown = ShootCooldown;
        player.ImmunityDuration = ImmunityDuration;
        player.AutoFire = AutoFire;
        player.Shotgun = Shotgun;
        
        player.HealthBar.InitializeHealthBar(MaxHealth);
    }

    public static void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.statType)
        {
            case StatType.MaxHealth:
                MaxHealth += 25;
                break;
            case StatType.HealthRegen:
                HealthRegen += 1;
                break;
            case StatType.MovementSpeed:
                MovementSpeed += 1.25f;
                break;
            case StatType.DashCooldown:
                DashCooldown = Mathf.Max(0.1f, DashCooldown - 0.1f);
                break;
            case StatType.ShootCooldown:
                ShootCooldown = Mathf.Max(0.1f, ShootCooldown - 0.1f);
                break;
            case StatType.BulletSpeed:
                BulletSpeed += 7.5f;
                break;
            case StatType.HighCalibre:
                BulletDamage += 20;
                ShootCooldown += 0.5f;
                break;
            case StatType.SMG:
                BulletDamage /= 2;
                BulletKnockbackForce /= 4f;
                ShootCooldown = ShootCooldown / 3;
                Spray = Mathf.Min(Spray * 2f, 45f);
                AutoFire = true;
                break;
            case StatType.Shotgun:
                BulletDamage = Mathf.RoundToInt(BulletDamage * 0.5f);
                BulletKnockbackForce *= 2f;
                Spray = Mathf.Min(Spray * 3f, 45f);
                Shotgun = true;
                break;
        }
        UpdatePlayerStats();
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public ObjectPool<Projectile> ProjectilePool;
    public static float EnemyHealthModifier { get; private set; } = 1f;
    public GameObject Player
    {
        get { return _player; }
    }
    GameObject _player;
    PlayerScript playerScript;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] UpgradeUIPanel uiPanel;
    [SerializeField] List<UpgradeData> allUpgrades;
    List<UpgradeData> pickedSingleUseUpgrades = new List<UpgradeData>();

    void Awake()
    {
        Instance = this;
        
        _player = GameObject.FindGameObjectWithTag("Player");
        playerScript = _player.GetComponent<PlayerScript>();
        
        ProjectilePool = new ObjectPool<Projectile>(
        createFunc: () => Instantiate(projectilePrefab).GetComponent<Projectile>(),
        actionOnGet: p => p.gameObject.SetActive(true),
        actionOnRelease: p => p.gameObject.SetActive(false),
        actionOnDestroy: p => Destroy(p.gameObject),
        defaultCapacity: 25,
        maxSize: 200
        );
    }

    public static void EndOfWave()
    {
        List<UpgradeData> options = GetRandomUpgrades(3);
        Instance.uiPanel.Show(options, OnUpgradeChosen);
        Instance.playerScript.enabled = false;
        Time.timeScale = 0f;
    }

    public static void OnUpgradeChosen(UpgradeData upgrade)
    {
        PlayerStatistics.ApplyUpgrade(upgrade);
        if (upgrade.oneTime)
            Instance.pickedSingleUseUpgrades.Add(upgrade);
        Instance.playerScript.Heal();
        Instance.playerScript.enabled = true;
        Time.timeScale = 1f;
    }

    static List<UpgradeData> GetRandomUpgrades(int count)
    {
        List<UpgradeData> pool = Instance.allUpgrades
            .Where(u => !Instance.pickedSingleUseUpgrades.Contains(u))
            .ToList();
        List<UpgradeData> selected = new List<UpgradeData>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            UpgradeData picked = PickWeighted(pool);
            selected.Add(picked);
            pool.Remove(picked);
        }

        return selected;
    }

    static UpgradeData PickWeighted(List<UpgradeData> pool)
    {
        int totalWeight = 0;
        foreach (var u in pool)
            totalWeight += 4 - u.rarity;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var u in pool)
        {
            cumulative += 4 - u.rarity;
            if (roll < cumulative)
                return u;
        }

        return pool[pool.Count - 1];
    }
}
