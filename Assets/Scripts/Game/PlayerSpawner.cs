using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private GameObject playerPrefab;
    private GameObject player;

    const string ALIVE = "alive";

    private void Start()
    {
        if (PhotonNetwork.IsConnected) SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        Transform sp = ServiceLocator.Resolve<ISpawnManager>().GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, sp.position, sp.rotation);

        var ht = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
        ht[ALIVE] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
    }

    public void Die(string damager)
    {
        ServiceLocator.Resolve<GameplayUIController>().DeathText = "You were killed by " + damager;
        
        var ht = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
        ht[ALIVE] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
        
        var role = (ServiceLocator.Resolve<ILocalRoleProvider>().LocalRole);
        if (role == RoleId.Assassin)
        {
            ServiceLocator.Resolve<IRoundService>().EndRound("INNOCENTS", "Assassin muerto");
            if (player) PhotonNetwork.Destroy(player);
            return;
        }
        
        if (player) PhotonNetwork.Destroy(player);
        ServiceLocator.Resolve<GameplayUIController>().DeathScreenActivate = true;
        FindObjectOfType<SpectatorController>(true)?.BeginSpectate();
    }
}