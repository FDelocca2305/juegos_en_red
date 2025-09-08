using System.Linq;
using UnityEngine;

public class PieceSpawnGroup : MonoBehaviour, IPieceSpawnProvider
{
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = GetComponentsInChildren<Transform>(true)
                .Where(t => t != transform).ToArray();
    }

    public Transform[] GetPieceSpawns() => spawnPoints;
}