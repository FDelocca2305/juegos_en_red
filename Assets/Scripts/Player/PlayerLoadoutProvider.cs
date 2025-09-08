using Photon.Pun;
using Player;
using UnityEngine;

public class PlayerLoadoutProvider : MonoBehaviourPunCallbacks
{
    [SerializeField] private BaseGun detectiveGun;
    [SerializeField] private BaseGun innocentPistol;
    
    [SerializeField] private ObjectivePaperToolItem paper;
    [SerializeField] private RadarToolItem radar;
    [SerializeField] private DetectiveDetectorToolItem detector;
    [SerializeField] private KnifeToolItem knife;

    private IPlayerInventory _inv;
    private IPlayerShootController _shoot;
    private ILocalRoleProvider _roles;

    private void Start()
    {
        if (!photonView.IsMine) return;

        ServiceLocator.TryResolve(out _inv);
        ServiceLocator.TryResolve(out _shoot);
        ServiceLocator.TryResolve(out _roles);

        Apply();
    }

    private void Apply()
    {
        var role = _roles.LocalRole;

        switch (role)
        {
            case RoleId.Detective:
                _inv.SetWeapon(null);
                _inv.TryAddTool(knife);
                _inv.TryAddTool(paper);
                _inv.SelectIndex(1);
                break;

            case RoleId.Assassin:
                _inv.SetWeapon(null);
                _inv.TryAddTool(knife);
                _inv.TryAddTool(paper);
                _inv.SelectIndex(1);
                break;

            case RoleId.Innocent:
                _inv.SetWeapon(null);
                _inv.TryAddTool(paper);
                _inv.SelectIndex(1);
                break;
        }
    }
}
