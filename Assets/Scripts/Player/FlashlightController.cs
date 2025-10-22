using System;
using Photon.Pun;
using UnityEngine;

public class FlashlightController : MonoBehaviourPunCallbacks, IFlashlightController
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject worldFlashlight;
    [SerializeField] private GameObject firstPersonLight;
    
    [Header("Config")]
    [SerializeField] private string animatorBool = "flashlight";
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    public bool IsOn { get; private set; }
    public event Action<bool> OnChanged;

    public override void OnEnable()
    {
        if (photonView.IsMine)
            ServiceLocator.Register<IFlashlightController>(this);
    }

    public override void OnDisable()
    {
        if (photonView.IsMine)
            ServiceLocator.Deregister<IFlashlightController>(this);
    }

    private void Start()
    {
        ApplyVisuals();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Toggle() => SetState(!IsOn);

    public void SetState(bool on)
    {
        if (IsOn == on) return;
        IsOn = on;

        ApplyVisuals();
        OnChanged?.Invoke(IsOn);
        
        photonView.RPC(nameof(RPC_SetState), RpcTarget.OthersBuffered, IsOn);
    }

    private void ApplyVisuals()
    {
        if (worldFlashlight) worldFlashlight.SetActive(IsOn);
        if (firstPersonLight) firstPersonLight.SetActive(photonView.IsMine && IsOn);
        if (animator) animator.SetBool(animatorBool, IsOn);
    }

    [PunRPC]
    private void RPC_SetState(bool on)
    {
        IsOn = on;
        ApplyVisuals();
    }
}