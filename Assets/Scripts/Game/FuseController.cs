using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FuseController : MonoBehaviourPun
{
    public enum PowerState { Normal, Blackout }
    public static FuseController I { get; private set; }
    public static bool Exists => I != null;

    [Header("Lights")]
    [SerializeField] private List<RoomLightGroup> allLightGroups = new();

    [Header("Fuse Spawn")]
    [SerializeField] private List<Transform> fuseSpawnPoints = new();
    [SerializeField] private GameObject fusePickupPrefab;

    [Header("Fuse Box")]
    [SerializeField] private Transform fuseBoxTarget;

    public PowerState Power { get; private set; } = PowerState.Normal;
    public int FuseHolderActor { get; private set; } = -1;

    GameObject fusePickupInstance;

    public event Action<PowerState> OnPowerChanged;
    public event Action<bool> OnLocalFuseHolderChanged;

    void Awake(){ I = this; }

    public void TriggerBlackout()
    {
        if (Power == PowerState.Blackout) return;
        if (PhotonNetwork.IsMasterClient) DoBlackoutMaster();
        else photonView.RPC(nameof(RpcRequestBlackout), RpcTarget.MasterClient);
    }

    [PunRPC] void RpcRequestBlackout()
    {
        if (PhotonNetwork.IsMasterClient) DoBlackoutMaster();
    }

    void DoBlackoutMaster()
    {
        photonView.RPC(nameof(RpcSetPower), RpcTarget.All, (int)PowerState.Blackout);
        SpawnFuseMaster();
    }

    public void RepairPower()
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(RpcSetPower), RpcTarget.All, (int)PowerState.Normal);
        else
            photonView.RPC(nameof(RpcRequestRepair), RpcTarget.MasterClient);
    }

    [PunRPC] void RpcRequestRepair()
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(RpcSetPower), RpcTarget.All, (int)PowerState.Normal);
    }

    [PunRPC] void RpcSetPower(int p)
    {
        Power = (PowerState)p;

        bool on = Power == PowerState.Normal;
        foreach (var g in allLightGroups) if (g) g.Set(on);

        if (on)
        {
            FuseHolderActor = -1;
            if (fusePickupInstance) { Destroy(fusePickupInstance); fusePickupInstance = null; }
        }

        OnPowerChanged?.Invoke(Power);
        OnLocalFuseHolderChanged?.Invoke(IsLocalHolder());
    }

    void SpawnFuseMaster()
    {
        if (fusePickupInstance) PhotonNetwork.Destroy(fusePickupInstance);
        if (fuseSpawnPoints.Count == 0) return;

        var t = fuseSpawnPoints[UnityEngine.Random.Range(0, fuseSpawnPoints.Count)];
        fusePickupInstance = PhotonNetwork.InstantiateRoomObject(fusePickupPrefab.name, t.position, t.rotation);
        FuseHolderActor = -1;
    }

    public void TryPickupFuse(int pickerActorNumber)
    {
        if (Power != PowerState.Blackout) return;
        if (FuseHolderActor != -1) return;

        if (PhotonNetwork.IsMasterClient)
            AssignFuseTo(pickerActorNumber);
        else
            photonView.RPC(nameof(RpcTryPickupFuse), RpcTarget.MasterClient, pickerActorNumber);
    }

    [PunRPC] void RpcTryPickupFuse(int actor) { if (PhotonNetwork.IsMasterClient) AssignFuseTo(actor); }

    void AssignFuseTo(int actor)
    {
        if (FuseHolderActor != -1) return;
        FuseHolderActor = actor;
        if (fusePickupInstance) { PhotonNetwork.Destroy(fusePickupInstance); fusePickupInstance = null; }
        photonView.RPC(nameof(RpcSetFuseHolder), RpcTarget.All, FuseHolderActor);
    }

    [PunRPC] void RpcSetFuseHolder(int actor)
    {
        FuseHolderActor = actor;
        OnLocalFuseHolderChanged?.Invoke(IsLocalHolder());
    }

    public bool IsLocalHolder() =>
        PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.ActorNumber == FuseHolderActor;

    public Transform FuseBoxTarget => fuseBoxTarget;
}
