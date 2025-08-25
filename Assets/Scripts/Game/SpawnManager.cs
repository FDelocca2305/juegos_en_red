using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour, ISpawnManager
{
    [SerializeField] private Transform[] spawnPoints;
    
    void Start()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            spawnPoint.gameObject.SetActive(false);
        }
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
