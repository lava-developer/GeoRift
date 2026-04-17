using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
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
    public List<SpawnWave> SpawnWaves;
    int waveToSpawn = 0;
    int enemiesLeft = 0;
    
    [SerializeField] TMP_Text waveIndicator;

    void Start()
    {
        StartCoroutine(GameLoop());
    }
    
    IEnumerator GameLoop()
    {
        while(waveToSpawn < SpawnWaves.Count)
        {
            waveIndicator.text = $"Wave: {waveToSpawn + 1}";
            yield return new WaitForSeconds(2);
            StartWave();
            yield return new WaitUntil(() => enemiesLeft <= 0);
            GameManager.EndOfWave();
        }
        waveIndicator.text = $"All waves cleared, congratulations!";
    }
    
    public void StartWave()
    {
        foreach (EnemySpawnData spawnData in SpawnWaves[waveToSpawn].spawnData)
        {
            enemiesLeft += spawnData.amount;
            StartCoroutine(SpawnEnemies(spawnData));
        }
        waveToSpawn++;
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
            Instantiate(spawnData.enemyPrefab, spawnData.spawnPoint.position, Quaternion.identity);
        }
    }
}
