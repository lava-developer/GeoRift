using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public List<Transform> spawnPoints;
    public int amount;
    public float interval;
}

[System.Serializable]
public class SpawnWave
{
    public List<EnemySpawnData> spawnData;
}

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }
    public List<SpawnWave> SpawnWaves;
    public int WaveToSpawn = 0;
    int enemiesLeft = 0;
    
    [SerializeField] TMP_Text waveIndicator;

    void Awake()
    {
        Instance = this;
        StartCoroutine(GameLoop());
    }
    
    IEnumerator GameLoop()
    {
        while(WaveToSpawn < SpawnWaves.Count)
        {
            waveIndicator.text = $"Wave: {WaveToSpawn + 1}";
            yield return new WaitForSeconds(2);
            StartWave();
            yield return new WaitUntil(() => enemiesLeft <= 0);
            yield return new WaitForSeconds(2);
            GameManager.EndOfWave();
        }
        waveIndicator.text = $"All waves cleared, congratulations!";
    }
    
    public void StartWave()
    {
        foreach (EnemySpawnData spawnData in SpawnWaves[WaveToSpawn].spawnData)
        {
            enemiesLeft += spawnData.amount;
            StartCoroutine(SpawnEnemies(spawnData));
        }
        WaveToSpawn++;
    }

    public void EnemyDeath()
    {
        enemiesLeft--;
    }

    IEnumerator SpawnEnemies(EnemySpawnData spawnData)
    {
        for (int i = 0; i < spawnData.amount; i++)
        {
            yield return new WaitForSeconds(spawnData.interval);
            Instantiate(spawnData.enemyPrefab, spawnData.spawnPoints[Random.Range(0, spawnData.spawnPoints.Count)].position, Quaternion.identity);
        }
    }
}
