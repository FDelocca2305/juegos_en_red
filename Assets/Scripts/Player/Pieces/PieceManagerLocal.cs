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
        while (RoomKeys.GetPhase() != RoomKeys.Phase_Playing) yield return null;
        while (!ServiceLocator.TryResolve(out _roles)) yield return null;

        ServiceLocator.TryResolve(out _tracker);
        ServiceLocator.TryResolve(out _ui);
        SpawnForLocal();
    }

    private static bool TryToInt(object o, out int val)
    {
        switch (o)
        {
            case int i: val = i; return true;
            case byte b: val = b; return true;
            case sbyte sb: val = sb; return true;
            case short s: val = s; return true;
            case ushort us: val = us; return true;
            case uint ui: val = (int)ui; return true;
            case long l: val = (int)l; return true;
            case ulong ul: val = (int)ul; return true;
            case float f: val = (int)f; return true;
            case double d: val = (int)d; return true;
            default:
                try { val = System.Convert.ToInt32(o); return true; }
                catch { val = 0; return false; }
        }
    }

    private RoleId GetLocalRoleNow()
    {
        var cp = PhotonNetwork.LocalPlayer?.CustomProperties;
        if (cp != null && cp.TryGetValue(RoleManager.RoleKey, out var v) && TryToInt(v, out var i))
            return (RoleId)i;

        return _roles?.LocalRole ?? RoleId.Innocent;
    }

    private void SpawnForLocal()
    {
        var points = _spawnPoints.OrderBy(_ => Random.value).Take(6).ToArray();

        var titles = Enumerable.Range(1, 6).Select(i => $"Piece {i}").ToArray();
        _tracker?.SetObjectives(titles);

        RoleId role = GetLocalRoleNow();
        var prefab = GetPrefabFor(role);

        for (int i = 0; i < points.Length; i++)
        {
            var p = Instantiate(prefab, points[i].position, points[i].rotation);
            p.gameObject.layer = LayerMask.NameToLayer("Interactable");
            p.Init(this, i, titles[i]);
        }

        _ui?.ShowHint($"Objectives created for role {role}", 1.0f);
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
        var role = GetLocalRoleNow();

        switch (role)
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
                inv.SelectIndex(0, true);
                ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>()?.ShowHint("Pistol Found", 1.2f);
                break;
        }
    }
}
