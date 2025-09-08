using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UI.Gameplay;
using UnityEngine;

public class PieceManagerLocal : MonoBehaviourPunCallbacks
{
    [SerializeField] private PieceInteractable assassinPiece;
    [SerializeField] private PieceInteractable detectivePiece;
    [SerializeField] private PieceInteractable innocentPiece;
    
    [SerializeField] private RadarToolItem radar;
    [SerializeField] private DetectiveDetectorToolItem detector;
    [SerializeField] private BaseGun innocentPistol;

    private ILocalRoleProvider _roles;
    private IObjectivesTracker _tracker;
    private IGameplayUI _ui;
    private Transform[] _spawnPoints;
    private int _collected;
    
    
    private IEnumerator Start()
    {
        if (!photonView.IsMine) { enabled = false; yield break; }
        
        yield return ServiceLocatorUtil.WaitFor<IPieceSpawnProvider>(p => _spawnPoints = p.GetPieceSpawns());

        ServiceLocator.TryResolve(out _roles);
        ServiceLocator.TryResolve(out _tracker);
        ServiceLocator.TryResolve(out _ui);
        SpawnForLocal();
    }

    private void SpawnForLocal()
    {
        var points = _spawnPoints.OrderBy(_ => Random.value).Take(6).ToArray();
        var pick6 = points.Take(6).ToArray();
        
        var titles = Enumerable.Range(1, 6).Select(i => $"Pieza {i}").ToArray();
        _tracker?.SetObjectives(titles);

        var prefab = GetPrefabFor(_roles.LocalRole);
        for (int i = 0; i < pick6.Length; i++)
        {
            var p = Instantiate(prefab, pick6[i].position, pick6[i].rotation);
            p.gameObject.layer = LayerMask.NameToLayer("Interactable");
            p.Init(this, i, titles[i]);
        }
    }

    private PieceInteractable GetPrefabFor(RoleId r) =>
        r == RoleId.Assassin ? assassinPiece :
        r == RoleId.Detective ? detectivePiece : innocentPiece;

    public void OnPieceCollected(int index, string title)
    {
        _collected++;
        _tracker?.MarkCompleted(index);
        _ui?.ShowHint($"Collected {title} ({_collected}/6)", 1f);

        if (_collected >= 6) GrantReward();
    }

    private void GrantReward()
    {
        switch (_roles.LocalRole)
        {
            case RoleId.Assassin:
                ServiceLocator.Resolve<IPlayerInventory>()?.TryAddTool(radar);
                ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()?.ShowHint("Radar Found", 1.2f);
                break;

            case RoleId.Detective:
                ServiceLocator.Resolve<IPlayerInventory>()?.TryAddTool(detector);
                ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()?.ShowHint("Detector Found (x2)", 1.2f);
                break;

            case RoleId.Innocent:
                var inv = ServiceLocator.Resolve<IPlayerInventory>();
                inv.SetWeapon(innocentPistol);
                var shoot = ServiceLocator.Resolve<IPlayerShootController>();
                shoot.SetMaxBullets(1);
                shoot.SetActualBullets(1);
                inv.SelectIndex(0);
                ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()?.ShowHint("Pistol Found", 1.2f);
                break;
        }
    }
}
