using System.Collections;
using Photon.Pun;
using Player;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerLoadoutProvider : MonoBehaviourPunCallbacks
{
    [Header("Weapon/Tools (puede ser instancia hija del player o prefab)")]
    [SerializeField] private BaseGun detectiveGun;
    [SerializeField] private BaseGun innocentPistol;

    [SerializeField] private ObjectivePaperToolItem paper;
    [SerializeField] private RadarToolItem radar;
    [SerializeField] private DetectiveDetectorToolItem detector;
    [SerializeField] private KnifeToolItem knife;

    private IPlayerInventory _inv;
    private IPlayerShootController _shoot;
    private ILocalRoleProvider _roles;

    private Transform _itemsParent;

    private void Awake()
    {
        _itemsParent = transform; // donde colgamos instancias si vienen como prefab
    }

    private IEnumerator Start()
    {
        if (!photonView.IsMine) yield break;

        // Esperar a que existan servicios
        while (!ServiceLocator.TryResolve(out _inv))   yield return null;
        while (!ServiceLocator.TryResolve(out _shoot)) yield return null;
        while (!ServiceLocator.TryResolve(out _roles)) yield return null;

        // Esperar a que la sala esté en Phase_Playing y que RoleManager haya marcado roles_assigned
        while (!PhotonNetwork.InRoom) yield return null;
        while (RoomKeys.GetPhase() != RoomKeys.Phase_Playing) yield return null;
        while (!RolesAssigned()) yield return null;

        ApplyLoadout();
    }

    private static bool RolesAssigned()
    {
        var room = PhotonNetwork.CurrentRoom;
        return room?.CustomProperties != null &&
               room.CustomProperties.TryGetValue(RoleManager.AssignedKey, out var v) &&
               v is bool b && b;
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changed)
    {
        if (!photonView.IsMine) return;
        if (target == PhotonNetwork.LocalPlayer && changed.ContainsKey(RoleManager.RoleKey))
            ApplyLoadout();
    }
    
    private RoleId GetLocalRoleNow()
    {
        var cp = PhotonNetwork.LocalPlayer.CustomProperties;
        if (cp != null && cp.TryGetValue(RoleManager.RoleKey, out var v) && v is int i)
            return (RoleId)i;
        return _roles?.LocalRole ?? RoleId.Innocent;
    }

    private void ApplyLoadout()
    {
        if (!photonView.IsMine || _inv == null || _roles == null) return;

        // limpiar inventario actual
        _inv.SetWeapon(null);
        _inv.ClearTools();

        var role = GetLocalRoleNow();
        Debug.Log($"[Loadout] Applying for role={role}");

        switch (role)
        {
            case RoleId.Detective:
                {
                    _inv.SetWeapon(detectiveGun);
                    _inv.TryAddTool(paper);
                    
                    if (detectiveGun != null)
                    {
                        _shoot.SetMaxBullets(detectiveGun.MaxBullets);
                        _shoot.SetActualBullets(detectiveGun.ActualBullets);
                    }
                    
                    _inv.SelectIndex(1);
                }
                break;

            case RoleId.Assassin:
                {
                    _inv.TryAddTool(knife);
                    _inv.TryAddTool(paper);
                    _inv.SelectIndex(1);
                }
                break;

            case RoleId.Innocent:
                {
                    _inv.TryAddTool(paper);
                    _inv.SelectIndex(1);
                }
                break;
        }
    }
}
