using System.Collections.Generic;
using UnityEngine;

public static class PlayerStatistics
{
    public static int MaxHealth = 100;
    public static float MovementSpeed = 6f;
    public static float KnockbackForce = 30f;
    public static float ShootCooldown = 0.5f;
    public static float ImmunityDuration = 0.25f;

    public static void UpdatePlayerStats()
    {
        PlayerScript player = GameManager.Instance.Player.GetComponent<PlayerScript>();

        player.MaxHealth = MaxHealth;
        player.MovementSpeed = MovementSpeed;
        player.KnockbackForce = KnockbackForce;
        player.ShootCooldown = ShootCooldown;
        player.ImmunityDuration = ImmunityDuration;
        
        player.HealthBar.InitializeHealthBar(MaxHealth);
    }

    public static void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.statType)
        {
            case StatType.MaxHealth:
                MaxHealth += Mathf.RoundToInt(upgrade.value);
                break;
            case StatType.MovementSpeed:
                MovementSpeed += upgrade.value;
                break;
            case StatType.KnockbackForce:
                KnockbackForce += upgrade.value;
                break;
            case StatType.ShootCooldown:
                ShootCooldown = Mathf.Max(0.1f, ShootCooldown - upgrade.value);
                break;
            case StatType.ImmunityDuration:
                ImmunityDuration += upgrade.value;
                break;
        }
        UpdatePlayerStats();
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static float EnemyHealthModifier { get; private set; } = 1f;
    public GameObject Player
    {
        get { return _player; }
    }
    GameObject _player;

    [SerializeField] UpgradeUIPanel uiPanel;
    [SerializeField] List<UpgradeData> allUpgrades;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    public static void EndOfWave()
    {
        List<UpgradeData> options = GetRandomUpgrades(3);
        Instance.uiPanel.Show(options, OnUpgradeChosen);
        Time.timeScale = 0f;
    }

    public static void OnUpgradeChosen(UpgradeData upgrade)
    {
        PlayerStatistics.ApplyUpgrade(upgrade);
        Time.timeScale = 1f;
    }

    static List<UpgradeData> GetRandomUpgrades(int count)
    {
        List<UpgradeData> pool = new List<UpgradeData>(Instance.allUpgrades);
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
