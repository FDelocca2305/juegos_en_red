using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(PhotonView))]
public class CandleInteractable : MonoBehaviourPun, IInteractable
{
    [Header("Visual")]
    [SerializeField] private GameObject fireModel;

    public bool IsLit { get; private set; }

    private ILocalFlashlightState _flashlight;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (fireModel) fireModel.SetActive(IsLit);
    }

    void Start()
    {
        ServiceLocator.TryResolve(out _flashlight);
    }
    
    public bool CanInteract()
    {
        return !IsLit && _flashlight != null && _flashlight.IsOn;
    }

    public string GetPrompt()
    {
        if (IsLit) return "";

        if (_flashlight == null || !_flashlight.IsOn)
            return "Needs fire to lit";

        return "Blaze it";
    }

    public void Interact(int? ownerActorNumber = null)
    {
        if (!CanInteract()) return;
        photonView.RPC(nameof(RPC_SetLit), RpcTarget.AllBuffered, true);
    }

    [PunRPC]
    void RPC_SetLit(bool on)
    {
        if (IsLit == on) return;
        IsLit = on;
        if (fireModel) fireModel.SetActive(IsLit);

        OuijaCandleRitualController.I?.OnCandleStateChanged();
    }
}