using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    public List<SpawnWave> spawnWaves;
    SpawnWave currentWave;

    [ContextMenu("Spawn Wave")]
    void DebugEnemySpawn()
    {
        StartWave();
    }

    public void StartWave()
    {
        currentWave = spawnWaves[0];
        foreach (EnemySpawnData spawnData in currentWave.spawnData)
        {
            StartCoroutine(SpawnEnemies(spawnData));
        }
        spawnWaves.RemoveAt(0);
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
