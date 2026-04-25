using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class EnemyDefinition
{
    public GameObject enemyPrefab;
    public List<Transform> spawnPoints;
    [Tooltip("Cost")]   public int cost = 1;
    [Tooltip("From which wave available")] public int unlockWave = 1;
    [Tooltip("Max units per wave")] public int maxPerWave = 0;
    public float spawnInterval = 0.8f;
}

public class EnemySpawnerEndless : MonoBehaviour, IEnemySpawner
{
    public static EnemySpawnerEndless Instance { get; private set; }

    public int CurrentWave => currentWave;

    public void EnemyDeath() => enemiesLeft--;

    public void Disable() => gameObject.SetActive(false);

    [Header("Enemy Definitions")]
    public List<EnemyDefinition> enemyDefinitions;

    [Header("Difficulty Scaling")]
    [SerializeField] int baseBudget = 6;
    [SerializeField] int budgetPerWave = 4;
    [SerializeField] float budgetExponent = 1.08f;

    [Header("UI")]
    [SerializeField] TMP_Text waveIndicator;

    int currentWave = 0;
    int enemiesLeft = 0;

    void Awake()
    {
        Instance = this;
        EnemySpawnerRegistry.Register(this);
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            currentWave++;
            waveIndicator.text = $"Wave: {currentWave}";

            yield return new WaitForSeconds(2f);

            StartWave(GenerateWave(currentWave));

            yield return new WaitUntil(() => enemiesLeft <= 0);

            yield return new WaitForSeconds(2f);

            GameManager.EndOfWave();
            yield return new WaitUntil(() => Time.timeScale == 1f);
        }
    }

    List<(EnemyDefinition def, int amount)> GenerateWave(int wave)
    {
        int budget = Mathf.RoundToInt(
            (baseBudget + budgetPerWave * wave) * Mathf.Pow(budgetExponent, wave)
        );

        var available = enemyDefinitions.FindAll(e =>
            e.unlockWave <= wave && e.enemyPrefab != null);

        var selected = new Dictionary<EnemyDefinition, int>();

        int safety = 2000;
        while (budget > 0 && safety-- > 0)
        {
            var affordable = available.FindAll(e =>
            {
                if (e.cost > budget) return false;
                if (e.maxPerWave > 0 &&
                    selected.TryGetValue(e, out int count) &&
                    count >= e.maxPerWave) return false;
                return true;
            });

            if (affordable.Count == 0) break;

            var pick = WeightedPick(affordable, wave);
            selected[pick] = selected.GetValueOrDefault(pick) + 1;
            budget -= pick.cost;
        }

        var result = new List<(EnemyDefinition, int)>();
        foreach (var kv in selected)
            result.Add((kv.Key, kv.Value));

        return result;
    }

    EnemyDefinition WeightedPick(List<EnemyDefinition> pool, int wave)
    {
        float[] weights = new float[pool.Count];
        float total = 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            float maturity = Mathf.Clamp01((wave - pool[i].unlockWave) / 8f);
            float w = Mathf.Lerp(1f / Mathf.Max(1, pool[i].cost), pool[i].cost * 0.4f, maturity);
            weights[i] = w;
            total += w;
        }

        float rand = Random.Range(0f, total);
        float cum  = 0f;
        for (int i = 0; i < pool.Count; i++)
        {
            cum += weights[i];
            if (rand <= cum) return pool[i];
        }
        return pool[^1];
    }

    void StartWave(List<(EnemyDefinition def, int amount)> composition)
    {
        enemiesLeft = 0;
        foreach (var (def, amount) in composition)
        {
            enemiesLeft += amount;
            StartCoroutine(SpawnEnemies(def, amount));
        }
    }

    IEnumerator SpawnEnemies(EnemyDefinition def, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(def.spawnInterval);
            var point = def.spawnPoints[Random.Range(0, def.spawnPoints.Count)];
            Instantiate(def.enemyPrefab, point.position, Quaternion.identity);
        }
    }
}