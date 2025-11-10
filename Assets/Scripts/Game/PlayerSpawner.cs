using System.Collections;
using Photon.Pun;
using UI.Gameplay;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerSpawner : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject canvasObjectives;
    
    private GameObject player;
    private bool _spawned;

    private IEnumerator Start()
    {
        while (!PhotonNetwork.InRoom) yield return null;
        
        while (RoomKeys.GetPhase() != RoomKeys.Phase_Playing) yield return null;
        while (!RolesAssigned()) yield return null;

        ISpawnManager spm = null;
        while (!ServiceLocator.TryResolve(out spm)) yield return null;

        TrySpawn(spm);
    }

    private static bool RolesAssigned()
    {
        var room = PhotonNetwork.CurrentRoom;
        return room?.CustomProperties != null &&
               room.CustomProperties.TryGetValue(RoleManager.AssignedKey, out var v) &&
               v is bool b && b;
    }

    private void TrySpawn(ISpawnManager spm)
    {
        if (_spawned || spm == null) return;
        var sp = spm.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, sp.position, sp.rotation);
        _spawned = true;

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { [RoomKeys.ALIVE] = true }
        );
    }

    public void SpawnPlayer()
    {
        if (!PhotonNetwork.InRoom) return;
        if (ServiceLocator.TryResolve(out ISpawnManager spm)) TrySpawn(spm);
    }

    public void Die(string damager)
    {
        canvasObjectives.SetActive(false);
        ServiceLocator.Resolve<IGameplayUI>().DeathText = "You were killed by " + damager;
        AudioManager.Instance?.PlayLocalSound("player_death_local");

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { [RoomKeys.ALIVE] = false }
        );

        var role = ServiceLocator.Resolve<ILocalRoleProvider>().LocalRole;
        if (role == RoleId.Assassin)
        {
            ServiceLocator.Resolve<IRoundService>().EndRound("INNOCENTS", "Assassin dead");
            if (player) PhotonNetwork.Destroy(player);
            return;
        }

        if (player) PhotonNetwork.Destroy(player);
        ServiceLocator.Resolve<IGameplayUI>().DeathScreenActivate = true;
        FindObjectOfType<SpectatorController>(true)?.BeginSpectate();
    }
}