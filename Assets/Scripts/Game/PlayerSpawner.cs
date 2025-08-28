using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private GameObject playerPrefab;
    private GameObject player;
    
    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPosition = ServiceLocator.Resolve<ISpawnManager>().GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition.position, spawnPosition.rotation);
    }
}
