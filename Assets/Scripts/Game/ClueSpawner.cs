using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ClueSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject cluePrefab;
    [SerializeField] private int clueCount = 10;

    private readonly List<GameObject> spawned = new();

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var points = new List<Transform>(spawnPoints);
        for (int i = 0; i < clueCount && points.Count > 0; i++)
        {
            int idx = Random.Range(0, points.Count);
            var t = points[idx];
            points.RemoveAt(idx);
            var go = PhotonNetwork.InstantiateRoomObject(cluePrefab.name, t.position, t.rotation);
            spawned.Add(go);
        }
    }
}