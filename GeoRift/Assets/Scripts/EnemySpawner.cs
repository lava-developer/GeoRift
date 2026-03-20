using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    int currentWave = 0;
    int enemiesLeft = 0;

    [ContextMenu("Spawn Wave")]
    void DebugEnemySpawn()
    {
        StartWave();
    }

    public void StartWave()
    {
        foreach (EnemySpawnData spawnData in SpawnWaves[currentWave].spawnData)
        {
            enemiesLeft += spawnData.amount;
            StartCoroutine(SpawnEnemies(spawnData));
        }
        currentWave++;
    }

    public void EnemyDeath()
    {
        enemiesLeft--;
        
        if (enemiesLeft <= 0)
        {
            enemiesLeft = 0;
            GameManager.EndOfWave();
        }
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
